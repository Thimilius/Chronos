using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chronos.Execution {
    public partial class ExecutionEngine {
        /// <summary>
        /// Makes a runtime method call.
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="callData">A reference to the method call data</param>
        private void MakeRuntimeCall(MethodDescription method, ref MethodCallData callData) {
            TypeDescription type = method.OwningType;
            string methodName = method.Name;
            if (type.IsDelegate) {
                switch (methodName) {
                    case ".ctor":
                        DelegateNew(ref callData);
                        break;
                    case "Invoke":
                        // NOTE: Because of the fact delegate and function pointers are using an index
                        // and not a real callable address, a potential interaction with native code would require a lot of work.
                        DelegateInvoke(ref callData);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (type.IsMDArray) {
                ArrayTypeDescription arrayType = type as ArrayTypeDescription;

                switch (methodName) {
                    case ".ctor":
                        MDArrayNew(arrayType, ref callData);
                        break;
                    case "Get":
                        MDArrayGet(arrayType, ref callData);
                        break;
                    case "Set":
                        MDArraySet(arrayType, ref callData);
                        break;
                    case "Address":
                        MDArrayAddress(arrayType, ref callData);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else {
                Debug.Assert(false);
            }
        }

        /// <summary>
        /// Runtime implementation for the Delegate constructor.
        /// </summary>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void DelegateNew(ref MethodCallData callData) {
            // Delegate constructors always have the following signature: .ctor(object, IntPtr).
            Debug.Assert(callData.ArgumentCount == 3);

            DelegateObject* del = (DelegateObject*)callData.GetArgument<IntPtr>(0);
            del->Target = (ObjectBase*)callData.GetArgument<IntPtr>(1);
            del->MethodHandle = GCHandle.FromIntPtr(callData.GetArgument<IntPtr>(2));

            ArrayTypeDescription arrayType = MetadataSystem.GetArrayType(MetadataSystem.MulticastDelegateType, 1);
            ArrayObject* array = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, 1);
            IntPtr *buffer = ObjectModel.GetArrayBuffer<IntPtr>(array);
            *buffer = new IntPtr(del);

            del->InvocationList = array;
        }  

        /// <summary>
        /// Runtime implementation for the Delegate.Invoke.
        /// </summary>
        /// <param name="invokeCallData">The call data of the invoke method</param>
        private unsafe void DelegateInvoke(ref MethodCallData invokeCallData) {
            DelegateObject* baseDelegate = (DelegateObject*)invokeCallData.GetArgument<IntPtr>(0);

            // When invoking a delegate we need to loop through the invocation list and invoke every delegate individually.
            for (int i = 0; i < baseDelegate->InvocationList->Length; i++) {
                IntPtr* buffer = ObjectModel.GetArrayBuffer<IntPtr>(baseDelegate->InvocationList);
                DelegateObject* itemDelegate = (DelegateObject*)buffer[i];

                InvokeDelegateItem(itemDelegate, ref invokeCallData);
            }
        }

        private unsafe void InvokeDelegateItem(DelegateObject* itemDelegate, ref MethodCallData invokeCallData) {
            MethodDescription callable = itemDelegate->Method;

            // The very first argument will always be a pointer to the actual delegate object.
            // Everything that follows are arguments (including a potential 'this' pointer) for the method we want to invoke.

            bool isInstance = callable.Signature.Header.IsInstance;
            int argumentCount = isInstance ? invokeCallData.ArgumentCount : invokeCallData.ArgumentCount - 1;
            ArgumentInfo[] argumentTypes = new ArgumentInfo[argumentCount];

            // Copy the arguments for the method.
            // The potential 'this' pointer gets handles explicitly
            // We have to remember to set the new offsets correctly.
            int nextOffset = isInstance ? MethodCallData.SLOT_SIZE : 0;
            int argumentOffset = isInstance ? 1 : 0;
            for (int j = 0; j < argumentCount - argumentOffset; j++) {
                ArgumentInfo argumentType = invokeCallData.GetArgumentInfo(j + 1);
                int tempOffset = argumentType.Offset;
                argumentType.Offset = nextOffset;
                nextOffset = tempOffset;
                argumentTypes[j + argumentOffset] = argumentType;
            }
            int argumentsSize = isInstance ? invokeCallData.ArgumentSize : invokeCallData.ArgumentSize - IntPtr.Size;

            MethodCallData newCallData = new MethodCallData(StackAllocator, callable, argumentTypes, argumentsSize);
            // Now copy the actual arguments over to the new call data.
            if (invokeCallData.ArgumentCount > 1) {
                Unsafe.CopyBlock((void*)newCallData.GetArgumentAddress(argumentOffset), (void*)invokeCallData.GetArgumentAddress(1), (uint)argumentsSize);
            }
            if (isInstance) {
                ObjectBase* obj = itemDelegate->Target;
                IntPtr target = new IntPtr(obj);

                TypeDescription objectType = obj->Type;
                TypeDescription methodType = callable.OwningType;

                // Just like in a regular call, we have to make sure structs get a pointer to their data
                // as their 'this' pointer and not an actual object reference.
                RuntimeType runtimeType = RuntimeType.FromObjectReference(objectType);
                if ((objectType.IsStruct || objectType.IsPrimitive) && objectType == callable.OwningType) {
                    runtimeType = RuntimeType.FromReference(objectType);
                    byte* thisStub = ((byte*)obj) + ObjectBase.SIZE;
                    target = new IntPtr(thisStub);
                } else {
                    runtimeType = RuntimeType.FromObjectReference(obj->Type);
                }

                argumentTypes[0] = new ArgumentInfo() {
                    RuntimeType = runtimeType,
                    Offset = 0
                };
                newCallData.SetArgument(0, target);
            }

            try {
                MakeCall(callable, ref newCallData);

                // We need to copy over the return value.
                if (!invokeCallData.GetReturnType().Type.IsVoid) {
                    Unsafe.CopyBlock(invokeCallData.GetReturnValueAddress(), newCallData.GetReturnValueAddress(), (uint)invokeCallData.ReturnSize);
                }
            } finally {
                newCallData.Dispose();
            }
        }

        /// <summary>
        /// Runtime implementation for a MD array constructor.
        /// </summary>
        /// <param name="arrayType">The type of the array</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void MDArrayNew(ArrayTypeDescription arrayType, ref MethodCallData callData) {
            // Figure out the combined length in all dimensions.
            int length = 1;
            for (int i = 1; i < callData.ArgumentCount; i++) {
                length *= callData.GetArgument<int>(i);
            }

            MDArrayObject* array = VirtualMachine.GarbageCollector.AllocateNewMDArray(arrayType, length);
            // We need to set the lengths dynamically here.
            int* dimensionLength = &array->FirstLength;
            for (int i = 1; i < callData.ArgumentCount; i++) {
                *dimensionLength = callData.GetArgument<int>(i);
                dimensionLength++;
            }

            callData.SetReturnValue(RuntimeType.FromObjectReference(arrayType), new IntPtr(array));
        }

        /// <summary>
        /// Runtime implementation for the MD array Get method.
        /// </summary>
        /// <param name="arrayType">The type of the array</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void MDArrayGet(ArrayTypeDescription arrayType, ref MethodCallData callData) {
            TypeDescription elementType = arrayType.ParameterType;
            MDArrayObject* array = (MDArrayObject*)callData.GetArgument<IntPtr>(0);
            ThrowOnInvalidPointer(array);

            byte* memory = ((byte*)array) + array->BUFFER_OFFSET;
            int index = ComputeMDArrayElementIndex(array, arrayType.Rank, ref callData);
            memory += elementType.GetVariableSize() * index;

            RuntimeType runtimeType = RuntimeType.FromType(elementType);

            if (elementType.IsEnum) {
                elementType = elementType.GetUnderlyingType();
            }

            if (elementType.IsSpecialSystemType) {
                switch (elementType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        callData.SetReturnValue(runtimeType, *(bool*)memory);
                        break;
                    case SpecialSystemType.Char:
                        callData.SetReturnValue(runtimeType, *(char*)memory);
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        callData.SetReturnValue(runtimeType, *(sbyte*)memory);
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        callData.SetReturnValue(runtimeType, *(short*)memory);
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        callData.SetReturnValue(runtimeType, *(int*)memory);
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        callData.SetReturnValue(runtimeType, *(long*)memory);
                        break;
                    case SpecialSystemType.Single:
                        callData.SetReturnValue(runtimeType, *(float*)memory);
                        break;
                    case SpecialSystemType.Double:
                        callData.SetReturnValue(runtimeType, *(double*)memory);
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        callData.SetReturnValue(runtimeType, *(IntPtr*)memory);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (elementType.IsStruct) {
                Unsafe.CopyBlock(callData.GetReturnValueAddress(), memory, (uint)elementType.Size);
            } else {
                // Everything else is a pointer.
                callData.SetReturnValue(runtimeType, *(IntPtr*)memory);
            }
        }

        /// <summary>
        /// Runtime implementation for the MD array Set method.
        /// </summary>
        /// <param name="arrayType">The type of the array</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void MDArraySet(ArrayTypeDescription arrayType, ref MethodCallData callData) {
            TypeDescription elementType = arrayType.ParameterType;
            MDArrayObject* array = (MDArrayObject*)callData.GetArgument<IntPtr>(0);
            ThrowOnInvalidPointer(array);

            byte* memory = ((byte*)array) + array->BUFFER_OFFSET;
            int index = ComputeMDArrayElementIndex(array, arrayType.Rank, ref callData);
            memory += elementType.GetVariableSize() * index;

            int argumentIndex = arrayType.Rank + 1;
            RuntimeType argumentRuntimeType = callData.GetArgumentType(argumentIndex);
            TypeDescription argumentType = argumentRuntimeType.Type;
            if (argumentRuntimeType.ItemType == StackItemType.ByReference || argumentRuntimeType.ItemType == StackItemType.ObjectReference) {
                *(IntPtr*)memory = callData.GetArgument<IntPtr>(argumentIndex);
            } else if (argumentType.IsSpecialSystemType) {
                switch (argumentType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        *(bool*)memory = callData.GetArgument<bool>(argumentIndex);
                        break;
                    case SpecialSystemType.Char:
                        *(char*)memory = callData.GetArgument<char>(argumentIndex);
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        *(sbyte*)memory = callData.GetArgument<sbyte>(argumentIndex);
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        *(short*)memory = callData.GetArgument<short>(argumentIndex);
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        *(int*)memory = callData.GetArgument<int>(argumentIndex);
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        *(long*)memory = callData.GetArgument<long>(argumentIndex);
                        break;
                    case SpecialSystemType.Single:
                        *(float*)memory = callData.GetArgument<float>(argumentIndex);
                        break;
                    case SpecialSystemType.Double:
                        *(double*)memory = callData.GetArgument<double>(argumentIndex);
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        *(IntPtr*)memory = callData.GetArgument<IntPtr>(argumentIndex);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (argumentType.IsStruct) {
                Unsafe.CopyBlock(memory, (void*)callData.GetArgumentAddress(argumentIndex), (uint)elementType.Size);
            } else {
                // Everything else is a pointer.
                *(IntPtr*)memory = callData.GetArgument<IntPtr>(argumentIndex);
            }
        }

        /// <summary>
        /// Runtime implementation for the MD array Address method.
        /// </summary>
        /// <param name="arrayType">The type of the array</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void MDArrayAddress(ArrayTypeDescription arrayType, ref MethodCallData callData) {
            TypeDescription elementType = arrayType.ParameterType;
            MDArrayObject* array = (MDArrayObject*)callData.GetArgument<IntPtr>(0);
            ThrowOnInvalidPointer(array);

            byte* address = ((byte*)array) + array->BUFFER_OFFSET;
            int index = ComputeMDArrayElementIndex(array, arrayType.Rank, ref callData);
            address += elementType.GetVariableSize() * index;

            callData.SetReturnValue(RuntimeType.FromReference(elementType), new IntPtr(address));
        }

        /// <summary>
        /// Computes the element index for a multidimensional array with an arbitrary number of dimensions.
        /// </summary>
        /// <param name="array">The array to get the element index for</param>
        /// <param name="rank">The rank of the array</param>
        /// <param name="callData">A reference to the method call data</param>
        /// <returns>The element index for the array</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe int ComputeMDArrayElementIndex(MDArrayObject* array, int rank, ref MethodCallData callData) {
            int* dimensionLength = &array->FirstLength;
            int sum = 0;
            for (int i = 0; i < rank; i++) {
                int k = i;

                int mul = 1;
                for (int l = k + 1; l < rank; l++) {
                    mul *= dimensionLength[l];
                }

                int index = callData.GetArgument<int>(i + 1);
                if (index < 0 || index >= dimensionLength[i]) {
                    ThrowIndexOutOfRangeException();
                }

                sum += mul * index;
            }
            return sum;
        }
    }
}
