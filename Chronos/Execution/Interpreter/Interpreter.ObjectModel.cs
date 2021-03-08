using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Creates a new object of a certain type and pushes the result on the stack.
        /// 
        /// Stack: ..., arg1 ... argN -> ..., obj
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretNewObject(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.MethodDefinition || token.Kind == HandleKind.MemberReference);

            // Theorectially there are four different cases:
            //     1. Reference types (ordinary constructor, resulting object reference gets pushed)
            //     2. Value types (ordinary constructor, resulting value type instance gets pushed)
            //     3. Strings (pseudo constructor, resulting object reference gets pushed)
            //     4. Multi-dimensional arrays (pseudo constructor, resulting object reference gets pushed)

            MethodDescription constructor = m_TokenResolver.ResolveMethod(token);
            MethodSignatureDescription signature = constructor.Signature;
            TypeDescription type = constructor.OwningType;

            // If we create a new string we do not really call a typical constructor.
            // Instead we have a pseudo constructor that allocates the string and returns it.
            // This return type needs to be specifically overwritten here, so the internal pseudo constructor can be called properly.
            bool hasPseudoConstructor = type.SpecialSystemType == SpecialSystemType.String || type.IsMDArray;
            bool isStruct = type.IsStruct || type.IsPrimitive;
            bool isLargeStruct = type.IsLargeStruct;

            // For strings we have to manually overwrite the return type to be a string.
            TypeDescription returnTypeOverwrite = hasPseudoConstructor ? type : null;
            ArgumentInfo[] argumentTypes = PrepareArgumentsForCall(signature, false, out int argumentSize);
            MethodCallData callData = new MethodCallData(m_StackAllocator, constructor, argumentTypes, argumentSize, returnTypeOverwrite);
            SetArgumentsForCall(signature, ref callData, false, 1, 1);

            // Normal objects get allocated before the constructor gets called
            ObjectBase* obj = null;
            if (!hasPseudoConstructor && !isStruct) {
                obj = VirtualMachine.GarbageCollector.AllocateNewObject(type);
            }

            // We set the 'this' pointer for the constructor explplicitly.
            RuntimeType runtimeType = RuntimeType.FromType(type);
            callData.SetArgumentType(0, new ArgumentInfo() { 
                RuntimeType = runtimeType,
                Offset = 0
            });
            IntPtr objectPointer = (IntPtr)obj;
            if (isStruct) {
                if (isLargeStruct) {
                    // For large structs we need to reserve the needed space and get a pointer to it.
                    objectPointer = m_Frame.ReserveLargeStructSpace(runtimeType);
                    callData.SetArgument(0, objectPointer);
                } else {
                    // For regular sized structs we treat the object pointer as the memory for the struct itself.
                    callData.SetArgument(0, new IntPtr(&objectPointer));
                }
            } else {
                callData.SetArgument(0, objectPointer);
            }

            // Now we can make the actual call to the constructor (regular or pseudo).
            VirtualMachine.ExecutionEngine.MakeCall(constructor, ref callData);

            // The pseudo constructors for string return us the new allocated object.
            if (hasPseudoConstructor) {
                objectPointer = callData.GetReturnValue<IntPtr>();
            }
            // Large structs are already on the stack because we reserved the space there.
            if (!isLargeStruct) {
                m_Frame.Push(runtimeType, objectPointer);
            }

            callData.Dispose();
        }

        /// <summary>
        /// Casts an object to an object of a new type.
        /// 
        /// Stack: ..., obj -> ..., obj
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretCastClass(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription castType = m_TokenResolver.ResolveType(token);
            StackItemType itemType = m_Frame.PeekItemType(0);
            Debug.Assert(itemType == StackItemType.ObjectReference);
            ObjectBase* obj = (ObjectBase*)m_Frame.Peek<IntPtr>(0);

            ObjectModel.IsInstanceOf(obj, castType, true);
            
            // The stack does not change if the cast was successfull.
        }

        /// <summary>
        /// Checks whether or not an object is an instance of a certain type.
        /// 
        /// Stack: ..., obj -> ..., result
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretIsInstance(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription castType = m_TokenResolver.ResolveType(token);
            RuntimeType runtimeType = m_Frame.PeekType(0);
            Debug.Assert(runtimeType.ItemType == StackItemType.ObjectReference);
            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ObjectBase* obj = (ObjectBase*)pointer;

            if (ObjectModel.IsInstanceOf(obj, castType, false)) {
                m_Frame.Push(runtimeType, pointer);
            } else {
                m_Frame.Push(runtimeType, IntPtr.Zero);
            }
        }

        /// <summary>
        /// Loads a constant string onto the stack.
        /// 
        /// Stack ... -> ..., obj
        /// </summary>
        /// <param name="token">The token of the string</param>
        private void InterpretLoadString(UserStringHandle token) {
            IntPtr ptr = m_TokenResolver.ResolveString(token);

            m_Frame.Push(RuntimeType.String, ptr);
        }

        /// <summary>
        /// Initializes a value at an address.
        /// 
        /// Stack: ..., destination -> ...
        /// </summary>
        /// <param name="token">The token of the type of the value</param>
        private unsafe void InterpretInitObject(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            if (type.IsStruct) {
                Unsafe.InitBlock((void*)pointer, 0, (uint)type.Size);
            } else {
                // If its a regular reference type, we just set the reference.
                *(IntPtr*)pointer = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Loads a value from an andress onto the stack.
        /// 
        /// Stack: ..., source -> ..., value
        /// </summary>
        /// <param name="token">The token of the type of the value</param>
        private unsafe void InterpretLoadObject(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            if (type.IsStruct) {
                if (type.IsLargeStruct) {
                    m_Frame.PushLargeStructFrom(RuntimeType.FromType(type), (void*)pointer);
                } else {
                    m_Frame.Push(RuntimeType.FromType(type), *(long*)pointer);
                }
            } else {
                // If its a regular reference type, we just load the reference.
                m_Frame.Push(RuntimeType.FromObjectReference(type), *(IntPtr*)pointer);
            }
        }

        /// <summary>
        /// Copies a value from one address to another.
        /// 
        /// Stack: ..., destination, source -> ...
        /// </summary>
        /// <param name="token">The token of the type of the value</param>
        private unsafe void InterpretCopyObject(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            void* source = (void*)m_Frame.Pop<IntPtr>();
            void* destination = (void*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(source);
            ThrowOnInvalidPointer(destination);

            if (type.IsStruct) {
                Unsafe.CopyBlock(destination, source, (uint)type.Size);
            } else {
                // If its a regular reference type, we just set the reference.
                IntPtr reference = *(IntPtr*)source;
                *(IntPtr*)destination = reference;
            }
        }

        /// <summary>
        /// Stores a value from the stack at an address.
        /// 
        /// Stack: ..., destination, value -> ...
        /// </summary>
        /// <param name="token">The token of the type of the value</param>
        private unsafe void InterpretStoreObject(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            if (type.IsStruct) {
                if (type.IsLargeStruct) {
                    void* destination = (void*)m_Frame.Peek<IntPtr>(1);
                    ThrowOnInvalidPointer(destination);

                    m_Frame.PopLargeStructTo(destination);
                    m_Frame.Pop<IntPtr>();
                } else {
                    long value = m_Frame.Pop<long>();

                    void* destination = (void*)m_Frame.Pop<IntPtr>();
                    ThrowOnInvalidPointer(destination);

                    *(long*)destination = value;
                }
            } else {
                // If its a regular reference type, we just set the reference.
                IntPtr reference = m_Frame.Pop<IntPtr>();

                void* destination = (void*)m_Frame.Pop<IntPtr>();
                ThrowOnInvalidPointer(destination);

                *(IntPtr*)destination = reference;
            }
        }
    }
}
