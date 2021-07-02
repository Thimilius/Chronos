using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Pushes a local variable on the stack.
        /// 
        /// Stack: ... -> ..., value
        /// </summary>
        /// <param name="index">The index of the local variable</param>
        private void InterpretLoadLocal(int index) {
            m_Frame.LoadLocal(index);
        }

        /// <summary>
        /// Pushes the address of a local variable on the stack.
        /// 
        /// Stack: ... -> ..., address (managed)
        /// </summary>
        /// <param name="index">The index of the local variable</param>
        private void InterpretLoadLocalAddress(int index) {
            m_Frame.LoadLocalAddress(index);
        }

        /// <summary>
        /// Stores a value from the stack in a local variable.
        /// 
        /// Stack: ..., value -> ...
        /// </summary>
        /// <param name="index">The index of the local variable</param>
        private void InterpretStoreLocal(int index) {
            m_Frame.StoreLocal(index);
        }

        /// <summary>
        /// Pushes an argument on the stack.
        /// 
        /// Stack ... -> ..., value
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void InterpretLoadArgument(int index, ref MethodCallData callData) {
            RuntimeType argumentRuntimeType = callData.GetArgumentType(index);
            TypeDescription argumentType = argumentRuntimeType.Type;

            if (argumentRuntimeType.ItemType == StackItemType.ByReference || argumentRuntimeType.ItemType == StackItemType.ObjectReference) {
                m_Frame.Push(argumentRuntimeType, callData.GetArgument<IntPtr>(index));
            } else if (argumentType.IsSpecialSystemType) {
                switch (argumentType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<bool>(index));
                        break;
                    case SpecialSystemType.Char:
                        m_Frame.Push(argumentRuntimeType, (int)callData.GetArgument<char>(index));
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<sbyte>(index));
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<short>(index));
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<int>(index));
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<long>(index));
                        break;
                    case SpecialSystemType.Single:
                        m_Frame.Push(argumentRuntimeType, (double)callData.GetArgument<float>(index));
                        break;
                    case SpecialSystemType.Double:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<double>(index));
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        m_Frame.Push(argumentRuntimeType, callData.GetArgument<IntPtr>(index));
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (argumentType.IsStruct) {
                if (argumentType.IsLargeStruct) {
                    m_Frame.PushLargeStructFrom(argumentRuntimeType, (void*)callData.GetArgumentAddress(index));
                } else {
                    m_Frame.Push(argumentRuntimeType, callData.GetArgument<IntPtr>(index));
                }
            } else {
                // Everything else is a pointer.
                m_Frame.Push(argumentRuntimeType, callData.GetArgument<IntPtr>(index));
            }
        }

        /// <summary>
        /// Pushes the address of an argument on the stack.
        /// 
        /// Stack ... -> ..., address (managed)
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void InterpretLoadArgumentAddress(int index, ref MethodCallData callData) {
            m_Frame.Push(RuntimeType.FromReference(callData.GetArgumentType(0).Type), callData.GetArgumentAddress(index));
        }

        /// <summary>
        /// Stores a value from the stack in an argument.
        /// 
        /// Stack ..., value -> ...
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <param name="callData">A reference to the method call data</param>
        private unsafe void InterpretStoreArgument(int index, ref MethodCallData callData) {
            RuntimeType argumentRuntimeType = m_Frame.PeekType(index);
            TypeDescription argumentType = argumentRuntimeType.Type;

            if (argumentType.IsSpecialSystemType) {
                switch (argumentType.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        callData.SetArgument(index, m_Frame.Pop<bool>());
                        break;
                    case SpecialSystemType.Char:
                        callData.SetArgument(index, m_Frame.Pop<char>());
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        callData.SetArgument(index, m_Frame.Pop<sbyte>());
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        callData.SetArgument(index, m_Frame.Pop<short>());
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        callData.SetArgument(index, m_Frame.Pop<int>());
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        callData.SetArgument(index, m_Frame.Pop<long>());
                        break;
                    case SpecialSystemType.Single:
                        callData.SetArgument(index, (float)m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.Double:
                        callData.SetArgument(index, m_Frame.Pop<double>());
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                    case SpecialSystemType.Object:
                    case SpecialSystemType.String:
                    case SpecialSystemType.Array:
                    case SpecialSystemType.MulticastDelegate:
                    case SpecialSystemType.RuntimeTypeHandle:
                    case SpecialSystemType.Exception:
                        callData.SetArgument(index, m_Frame.Pop<IntPtr>());
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (argumentType.IsStruct) {
                if (argumentType.IsLargeStruct) {
                    void* slot = callData.SetLargeStructArgument(index);
                    m_Frame.PopLargeStructTo(slot);
                } else {
                    callData.SetArgument(index, m_Frame.Pop<IntPtr>());
                }
            } else {
                // Everything else is a pointer.
                callData.SetArgument(index, m_Frame.Pop<IntPtr>());
            }
        }

        /// <summary>
        /// Pushes the constant null object reference on the stack.
        /// 
        /// Stack: ... -> ..., obj (null)
        /// </summary>
        private void InterpretLoadNull() {
            m_Frame.Push(RuntimeType.Null, IntPtr.Zero);
        }

        /// <summary>
        /// Pushes an 32-bit integer constant on the stack.
        /// 
        /// Stack: ... -> ..., constant
        /// </summary>
        /// <param name="value">The 32-bit integer constant</param>
        private void InterpretLoadConstant(int value) {
            m_Frame.Push(RuntimeType.Int32, value);
        }

        /// <summary>
        /// Pushes an 64-bit integer constant on the stack.
        /// 
        /// Stack: ... -> ..., constant
        /// </summary>
        /// <param name="value">The 64-bit integer constant</param>
        private void InterpretLoadConstant(long value) {
            m_Frame.Push(RuntimeType.Int64, value);
        }

        /// <summary>
        /// Pushes a floating point constant on the stack.
        /// 
        /// Stack: ... -> ..., constant
        /// </summary>
        /// <param name="value">The floating point constant</param>
        private void InterpretLoadConstant(double value) {
            m_Frame.Push(RuntimeType.Double, value);
        }

        /// <summary>
        /// Duplicates the topmost stack item.
        /// 
        /// Stack: ..., value -> ..., value, value
        /// </summary>
        private unsafe void InterpretDuplicate() {
            RuntimeType type = m_Frame.PeekType(0);
            if (type.ItemType == StackItemType.ValueType && type.Type.IsLargeStruct) {
                m_Frame.PushLargeStructFrom(type, (void*)m_Frame.PeekLargeStructAddress());
            } else {
                m_Frame.Push(type, m_Frame.Peek<long>(0));
            }
        }

        /// <summary>
        /// Pops the topmost item from the stack.
        /// 
        /// Stack: ..., value -> ...
        /// </summary>
        private unsafe void InterpretPop() {
            RuntimeType type = m_Frame.PeekType(0);

            if (type.Type.IsLargeStruct) {
                m_Frame.PopLargeStruct();
            } else {
                m_Frame.Pop<long>();
            }
        }

        /// <summary>
        /// Pushes the address of a method on the stack.
        /// 
        /// Stack: ... -> ..., address
        /// </summary>
        /// <param name="token">The token of the method</param>
        private unsafe void InterpretLoadFunction(EntityHandle token) {
            MethodDescription method = m_TokenResolver.ResolveMethod(token);

            // NOTE: Loading a function pointer means pushing the index to the method on the stack.

            m_Frame.Push(RuntimeType.NativeInt, GCHandle.ToIntPtr(method.Handle));
        }

        /// <summary>
        /// Pushes the address of a virtual method on the stack.
        /// 
        /// Stack: ..., obj -> ..., address
        /// </summary>
        /// <param name="token">The token of the virtual method</param>
        private unsafe void InterpretLoadVirtualFunction(EntityHandle token) {
            MethodDescription method = m_TokenResolver.ResolveMethod(token);

            ObjectBase* obj = (ObjectBase*)m_Frame.Pop<IntPtr>();
            TypeDescription type = obj->Type;

            MethodDescription resolvedMethod = ObjectModel.ResolveVirtualMethod(method, type);

            // NOTE: Loading a function pointer means pushing the index to the method on the stack.
            m_Frame.Push(RuntimeType.NativeInt, GCHandle.ToIntPtr(resolvedMethod.Handle));
        }

        /// <summary>
        /// Pushes a value stored at an address on the stack.
        /// 
        /// Stack: ..., address (managed or unmanaged) -> ..., value
        /// </summary>
        /// <typeparam name="TFrom">The type of the value</typeparam>
        /// <typeparam name="TTo">The the value should be cast to</typeparam>
        /// <param name="type">The runtime type to push</param>
        private unsafe void InterpretLoadIndirect<TFrom, TTo>(RuntimeType type) where TFrom : unmanaged where TTo : unmanaged {
            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            TFrom temp = *(TFrom*)pointer;
            m_Frame.Push(type, Unsafe.As<TFrom, TTo>(ref temp));
        }

        /// <summary>
        /// Pushes a float stored at an address on the stack.
        /// 
        /// Stack: ..., address (managed or unmanaged) -> ..., value
        /// </summary>
        private unsafe void InterpretLoadIndirectFloat() {
            // NOTE: This is a direct copy of InterpretLoadIndirect<T> but we need the special float to double conversion

            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            m_Frame.Push(RuntimeType.Double, (double)*(float*)pointer);
        }

        /// <summary>
        /// Pushes an object reference at an address on the stack.
        /// 
        /// Stack: ..., address (managed or unmanaged) -> ..., value
        /// </summary>
        private unsafe void InterpretLoadIndirectRef() {
            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            IntPtr value = *(IntPtr*)pointer;
            ObjectBase* obj = (ObjectBase*)value;
            TypeDescription type = obj->Type;

            m_Frame.Push(RuntimeType.FromObjectReference(type), value);
        }

        /// <summary>
        /// Stores a value at an address.
        /// 
        /// Stack: ..., address (managed or unmanaged), value -> ...
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo">The the value should be cast to</typeparam>
        private unsafe void InterpretStoreIndirect<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged {
            TFrom temp = m_Frame.Pop<TFrom>();
            TTo value = Unsafe.As<TFrom, TTo>(ref temp);

            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            *(TTo*)pointer = value;
        }

        /// <summary>
        /// Stores a float at an address.
        /// 
        /// Stack: ..., address (managed or unmanaged), value -> ...
        /// </summary>
        private unsafe void InterpretStoreIndirectFloat() {
            // NOTE: This is a direct copy of InterpretStoreIndirect<T> but we need the special double to float conversion

            float value = (float)m_Frame.Pop<double>();

            IntPtr pointer = m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(pointer);

            *(float*)pointer = value;
        }
    }
}
