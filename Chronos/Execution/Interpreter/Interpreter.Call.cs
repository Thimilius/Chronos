using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Performs a function call and pushes an optional return value on the stack.
        /// 
        /// Stack ..., arg0 ... argN -> ..., return value
        /// </summary>
        /// <param name="token">The token of the method</param>
        /// <param name="isVirtualCall">Indicator if the call is virtual</param>
        private unsafe void InterpretCall(EntityHandle token, bool isVirtualCall) {
            Debug.Assert(token.Kind == HandleKind.MethodDefinition || token.Kind == HandleKind.MemberReference);
            
            MethodDescription method = m_TokenResolver.ResolveMethod(token);
            MethodSignatureDescription signature = method.Signature;
            bool isInstanceCall = signature.Header.IsInstance;

            // Prepare the argument types and offsets.
            ArgumentInfo[] argumentTypes = PrepareArgumentsForCall(signature, isInstanceCall, out int argumentSize);
            if (isInstanceCall) {
                // If we have an instance method, make sure we have a proper pointer.
                int thisPointerOffsetOnStack = argumentTypes.Length - 1;
                IntPtr thisPointer = m_Frame.Peek<IntPtr>(thisPointerOffsetOnStack);
                ThrowOnInvalidPointer(thisPointer);

                if (isVirtualCall) {
                    // We need the actual runtime type of the instance to resolve the virtual method properly.
                    RuntimeType instanceRuntimeType = m_Frame.PeekType(thisPointerOffsetOnStack);

                    // For virtual calls it might be that we have a constrained call.
                    bool needsToResolveVirtualMethod = true;
                    if (m_ConstrainedFlag) {
                        StackItemType stackItemType = m_Frame.PeekItemType(thisPointerOffsetOnStack);

                        // There are 3 cases that can occur on a constrained call.
                        if (m_ConstrainedType.IsReference) {
                            // For reference types the reference on the stack is actually a pointer to the pointer of the object.
                            // We dereference that and set it as the new 'this' pointer thereby removing the double indirection.
                            ObjectBase** pointerToPointer = (ObjectBase**)m_Frame.PeekAddress(thisPointerOffsetOnStack);
                            ThrowOnInvalidPointer(pointerToPointer);

                            ObjectBase* newThisPointer = *pointerToPointer;
                            m_Frame.Set(thisPointerOffsetOnStack, RuntimeType.FromObjectReference(instanceRuntimeType.Type), new IntPtr(newThisPointer));
                        } else {
                            if (method.OwningType == m_ConstrainedType) {
                                // This is the simplest case, as we do not need to do anything here.
                                // The value type implements the method to call, so we just pass the 'this' pointer on the stack as is.
                                needsToResolveVirtualMethod = false;
                            } else {
                                // For value types which do not implemented the method we want to call,
                                // we have to box the value at the reference and set the boxed object as the new 'this' pointer.
                                byte** pointerToPointer = (byte**)m_Frame.PeekAddress(thisPointerOffsetOnStack);
                                ThrowOnInvalidPointer(pointerToPointer);

                                ObjectBase* obj = ObjectModel.Box(m_ConstrainedType, *pointerToPointer);

                                m_Frame.Set(thisPointerOffsetOnStack, RuntimeType.FromObjectReference(m_ConstrainedType), new IntPtr(obj));
                            }
                        }

                        // We have to reset the constrained flag.
                        m_ConstrainedFlag = false;
                    }

                    if (needsToResolveVirtualMethod) {
                        // We peek at the 'this' pointer here again, as we might have replaced it on a constrained call earlier.
                        thisPointer = m_Frame.Peek<IntPtr>(thisPointerOffsetOnStack);
                        ThrowOnInvalidPointer(thisPointer);

                        // Figure out the actual type of the object and resolve the virtual method.
                        ObjectBase* obj = (ObjectBase*)thisPointer;
                        TypeDescription type = obj->Type;
                        method = ObjectModel.ResolveVirtualMethod(method, type);

                        // When we are making a call to a virtual method on a struct, we have to adjust the 'this' pointer.
                        // In that case the 'this' pointer becomes a reference to the actual struct data inside the boxed object.
                        // This can only occur on boxed structs that have overwritten a method (like ToString).
                        // That means we need to check the owner of the resolved virtual method
                        // to see if it matches with the type of the boxed struct.
                        if (type.IsStruct || type.IsPrimitive) {
                            if (type == method.OwningType) {
                                // We have to reset the 'this' pointer on the stack.
                                byte* thisStub = ((byte*)obj) + ObjectBase.SIZE;
                                RuntimeType thisRuntimeType = RuntimeType.FromReference(type);
                                m_Frame.Set(thisPointerOffsetOnStack, thisRuntimeType, new IntPtr(thisStub));

                                // Also we have to adjust the argument type.
                                argumentTypes[0] = new ArgumentInfo() {
                                    RuntimeType = thisRuntimeType,
                                    Offset = 0
                                };
                            }
                        }
                    }
                }
            }

            // Set all required arguments.
            MethodCallData callData = new MethodCallData(m_StackAllocator, method, argumentTypes, argumentSize);
            int offset = isInstanceCall ? 1 : 0;
            SetArgumentsForCall(signature, ref callData, isInstanceCall, offset, offset);

            // Make the actual method call.
            VirtualMachine.ExecutionEngine.MakeCall(method, ref callData);

            SetReturnFromCall(GetActualType(signature.ReturnType), ref callData);

            callData.Dispose();
        }

        /// <summary>
        /// Sets the return value from the stack.
        /// 
        /// Stack ..., value -> ...
        /// </summary>
        /// <param name="callData">The method call data</param>
        private unsafe void InterpretReturn(ref MethodCallData callData) {
            TypeDescription returnType = GetActualType(m_Method.Signature.ReturnType);
            if (returnType.IsVoid) {
                return;
            }

            RuntimeType runtimeType = m_Frame.PeekType(0);

            if (returnType.IsSpecialSystemType) {
                switch (returnType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<bool>());
                        break;
                    case SpecialSystemType.Char:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<char>());
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<sbyte>());
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<short>());
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<long>());
                        break;
                    case SpecialSystemType.Single:
                        callData.SetReturnValue(runtimeType, (float)m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.Double:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        callData.SetReturnValue(runtimeType, m_Frame.Pop<IntPtr>());
                        break;
                }
            } else if (returnType.IsStruct) {
                if (returnType.IsLargeStruct) {
                    m_Frame.PopLargeStructTo(callData.GetReturnValueAddress());
                } else {
                    callData.SetReturnValue(runtimeType, m_Frame.Pop<IntPtr>());
                }
            } else {
                // Everything else is a pointer.
                callData.SetReturnValue(runtimeType, m_Frame.Pop<IntPtr>());
            }
        }

        /// <summary>
        /// Prepares the arguments for a method call.
        /// </summary>
        /// <param name="signature">A reference to the signature of the method</param>
        /// <param name="argumentSize">The out parameter that will contain the size in bytes needed for the arguments</param>
        /// <param name="thisFromStack">Indicator for whether or not the 'this' pointer lies on the stack</param>
        /// <returns></returns>
        private ArgumentInfo[] PrepareArgumentsForCall(MethodSignatureDescription signature, bool thisFromStack, out int argumentSize) {
            // First we have to figure out how many actual arguments there are for us.
            bool isInstance = signature.Header.IsInstance;
            int argumentCount = 0;
            argumentSize = 0;
            if (isInstance) {
                argumentSize += MethodCallData.SLOT_SIZE;
                argumentCount++;
            }

            // Now we can set the type of the 'this' pointer, if we have one.
            argumentCount += signature.ParameterTypes.Count;
            ArgumentInfo[] argumentTypes = new ArgumentInfo[argumentCount];

            if (isInstance) {
                // We only want to set the 'this' pointer, if we got told it comes from the stack.
                if (thisFromStack) {
                    argumentTypes[0] = new ArgumentInfo() {
                        RuntimeType = m_Frame.PeekType(argumentCount - 1),
                        Offset = 0
                    };
                }
            }

            // Now we can set the types and offsets of all remaining arguments.
            int offset = isInstance ? 1 : 0;
            int argumentCountOffset = thisFromStack ? argumentCount : argumentCount - 1;
            for (int i = 0; i < signature.ParameterTypes.Count; i++) {
                TypeDescription argumentType = signature.ParameterTypes[i];

                // We only set the type from the stack if its a reference type, otherwise we can get the argument type from the signature.
                // It might be however that the reference is null, in which case we can also set the regular argument type from the signature.
                RuntimeType argumentRuntimeType;
                if (argumentType.IsReference) {
                    argumentRuntimeType = m_Frame.PeekType(argumentCountOffset - i - offset);
                    if (argumentRuntimeType.Type == null) {
                        argumentRuntimeType = RuntimeType.FromType(argumentType);
                    }
                } else {
                    argumentRuntimeType = RuntimeType.FromType(argumentType);
                }
                argumentTypes[i + offset] = new ArgumentInfo() {
                    RuntimeType = argumentRuntimeType,
                    Offset = argumentSize
                };

                if (argumentType.IsLargeStruct) {
                    argumentSize += argumentType.Size;
                } else {
                    argumentSize += MethodCallData.SLOT_SIZE;
                }
            }

            return argumentTypes;
        }

        /// <summary>
        /// Sets the argument values for a method call.
        /// </summary>
        /// <param name="signature">A reference to the signature of the method to call</param>
        /// <param name="callData">A reference to the method call data</param>
        /// <param name="thisFromStack">Indicator for whether or not he 'this' pointer lies on the stack</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void SetArgumentsForCall(MethodSignatureDescription signature, ref MethodCallData callData, bool thisFromStack, int bailOffset, int argumentOffset) {
            for (int argumentIndex = callData.ArgumentCount - 1; argumentIndex >= bailOffset; argumentIndex--) {
                TypeDescription argumentType = signature.ParameterTypes[argumentIndex - argumentOffset];
                
                if (argumentType.IsSpecialSystemType) {
                    switch (argumentType.SpecialSystemType) {
                        case SpecialSystemType.Boolean:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<bool>());
                            break;
                        case SpecialSystemType.Char:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<char>());
                            break;
                        case SpecialSystemType.SByte:
                        case SpecialSystemType.Byte:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<sbyte>());
                            break;
                        case SpecialSystemType.Int16:
                        case SpecialSystemType.UInt16:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<short>());
                            break;
                        case SpecialSystemType.Int32:
                        case SpecialSystemType.UInt32:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<int>());
                            break;
                        case SpecialSystemType.Int64:
                        case SpecialSystemType.UInt64:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<long>());
                            break;
                        case SpecialSystemType.Single:
                            callData.SetArgument(argumentIndex, (float)m_Frame.Pop<double>());
                            break;
                        case SpecialSystemType.Double:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<double>());
                            break;
                        case SpecialSystemType.IntPtr:
                        case SpecialSystemType.UIntPtr:
                        case SpecialSystemType.Object:
                        case SpecialSystemType.String:
                        case SpecialSystemType.Array:
                        case SpecialSystemType.MulticastDelegate:
                        case SpecialSystemType.RuntimeTypeHandle:
                        case SpecialSystemType.Exception:
                            callData.SetArgument(argumentIndex, m_Frame.Pop<IntPtr>());
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                } else if (argumentType.IsStruct) {
                    if (argumentType.IsLargeStruct) {
                        void* slot = callData.SetLargeStructArgument(argumentIndex);
                        m_Frame.PopLargeStructTo(slot);
                    } else {
                        callData.SetArgument(argumentIndex, m_Frame.Pop<IntPtr>());
                    }
                } else {
                    // Everything else is a pointer.
                    callData.SetArgument(argumentIndex, m_Frame.Pop<IntPtr>());
                }
            }

            // Set the 'this' pointer explicitly if needed.
            if (thisFromStack) {
                callData.SetArgument(0, m_Frame.Pop<IntPtr>());
            }
        }

        /// <summary>
        /// Pushes the return value of a method call on the stack.
        /// </summary>
        /// <param name="returnType">The return type</param>
        /// <param name="callData">A reference to the method call data</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private unsafe void SetReturnFromCall(TypeDescription returnType, ref MethodCallData callData) {
            if (!returnType.IsVoid) {
                RuntimeType type = callData.GetReturnType();

                if (returnType.IsSpecialSystemType) {
                    switch (returnType.SpecialSystemType) {
                        case SpecialSystemType.Boolean:
                            bool temp = callData.GetReturnValue<bool>();
                            m_Frame.Push(type, Unsafe.As<bool, int>(ref temp));
                            break;
                        case SpecialSystemType.Char:
                            m_Frame.Push(type, (int)callData.GetReturnValue<char>());
                            break;
                        case SpecialSystemType.SByte:
                        case SpecialSystemType.Byte:
                            m_Frame.Push(type, (int)callData.GetReturnValue<sbyte>());
                            break;
                        case SpecialSystemType.Int16:
                        case SpecialSystemType.UInt16:
                            m_Frame.Push(type, (int)callData.GetReturnValue<short>());
                            break;
                        case SpecialSystemType.Int32:
                        case SpecialSystemType.UInt32:
                            m_Frame.Push(type, callData.GetReturnValue<int>());
                            break;
                        case SpecialSystemType.Int64:
                        case SpecialSystemType.UInt64:
                            m_Frame.Push(type, callData.GetReturnValue<long>());
                            break;
                        case SpecialSystemType.Single:
                            m_Frame.Push(type, (double)callData.GetReturnValue<float>());
                            break;
                        case SpecialSystemType.Double:
                            m_Frame.Push(type, callData.GetReturnValue<double>());
                            break;
                        case SpecialSystemType.IntPtr:
                        case SpecialSystemType.UIntPtr:
                        case SpecialSystemType.Object:
                        case SpecialSystemType.String:
                        case SpecialSystemType.Array:
                        case SpecialSystemType.MulticastDelegate:
                        case SpecialSystemType.RuntimeTypeHandle:
                        case SpecialSystemType.Exception:
                            m_Frame.Push(type, callData.GetReturnValue<IntPtr>());
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                } else if (returnType.IsStruct) {
                    if (returnType.IsLargeStruct) {
                        m_Frame.PushLargeStructFrom(type, callData.GetReturnValueAddress());
                    } else {
                        m_Frame.Push(type, callData.GetReturnValue<IntPtr>());
                    }
                } else {
                    // Everything else is a pointer.
                    m_Frame.Push(type, callData.GetReturnValue<IntPtr>());
                }
            }
        }
    }
}
