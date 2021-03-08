using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Unboxes a value type and pushes a managed reference to it on the stack.
        /// 
        /// Stack: ..., obj -> ..., address (managed)
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretUnbox(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            ObjectBase* obj = (ObjectBase*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(obj);

            // We can only unbox.any primitives and value types.
            if (type.IsReference) {
                VirtualMachine.ExceptionEngine.ThrowInvalidCastException();
            }

            byte* data = ObjectModel.GetObjectData(obj);

            // NOTE: Currently no references are tracked by the garbage collector,
            // meaning the object the reference points into might get cleaned up at some point.
            // That is bad news.
            m_Frame.Push(RuntimeType.FromReference(type), new IntPtr(data));
        }

        /// <summary>
        /// Boxes a value to an object reference.
        /// 
        /// Stack: ..., value -> ..., obj
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretBox(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            IntPtr pointer = IntPtr.Zero;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            switch (stackItemType) {
                case StackItemType.Int32:
                case StackItemType.Int64:
                case StackItemType.NativeInt:
                case StackItemType.Double:
                    pointer = m_Frame.PeekAddress(0);
                    break;
                case StackItemType.ValueType:
                    if (type.IsLargeStruct) {
                        pointer = m_Frame.PeekLargeStructAddress();
                    } else {
                        pointer = m_Frame.PeekAddress(0);
                    }
                    break;
                case StackItemType.ObjectReference:
                    // We do nothing here, as boxing on a reference does not change anything.
                    // That means we can bail out early.
                    return;
                case StackItemType.None:
                case StackItemType.ByReference:
                default:
                    Debug.Assert(false);
                    break;
            }

            ObjectBase* obj = ObjectModel.Box(type, (byte*)pointer);

            // Now we can pop of the actual value.
            switch (stackItemType) {
                case StackItemType.Int32:
                case StackItemType.Int64:
                case StackItemType.NativeInt:
                case StackItemType.Double:
                    m_Frame.Pop<long>();
                    break;
                case StackItemType.ValueType:
                    if (type.IsLargeStruct) {
                        m_Frame.PopLargeStruct();
                    } else {
                        m_Frame.Pop<long>();
                    }
                    break;
                case StackItemType.ObjectReference:
                case StackItemType.None:
                case StackItemType.ByReference:
                default:
                    Debug.Assert(false);
                    break;
            }
            
            m_Frame.Push(RuntimeType.FromObjectReference(type), new IntPtr(obj));
        }

        /// <summary>
        /// Unboxes an object reference to its value.
        /// 
        /// Stack: ..., obj -> ..., value
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretUnboxAny(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);

            ObjectBase* obj = (ObjectBase*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(obj);

            // Make sure the types actually match.
            if (!ObjectModel.IsInstanceOf(obj, type, true)) {
                return;
            }

            RuntimeType runtimeType = RuntimeType.FromType(type);

            // Reference types just get pushed on the stack again as they are.
            if (type.IsReference) {
                m_Frame.Push(runtimeType, new IntPtr(obj));
            }

            byte* data = ObjectModel.GetObjectData(obj);
            if (type.IsSpecialSystemType) {
                switch (type.SpecialSystemType) {
                    case SpecialSystemType.Boolean:
                        bool value = *(bool*)data;
                        m_Frame.Push(runtimeType, Unsafe.As<bool, int>(ref value));
                        break;
                    case SpecialSystemType.Char:
                        m_Frame.Push(runtimeType, (int)*(char*)data);
                        break;
                    case SpecialSystemType.SByte:
                    case SpecialSystemType.Byte:
                        m_Frame.Push(runtimeType, (int)*(sbyte*)data);
                        break;
                    case SpecialSystemType.Int16:
                    case SpecialSystemType.UInt16:
                        m_Frame.Push(runtimeType, (int)*(short*)data);
                        break;
                    case SpecialSystemType.Int32:
                    case SpecialSystemType.UInt32:
                        m_Frame.Push(runtimeType, *(int*)data);
                        break;
                    case SpecialSystemType.Int64:
                    case SpecialSystemType.UInt64:
                        m_Frame.Push(runtimeType, *(long*)data);
                        break;
                    case SpecialSystemType.Single:
                        m_Frame.Push(runtimeType, (double)*(float*)data);
                        break;
                    case SpecialSystemType.Double:
                        m_Frame.Push(runtimeType, *(float*)data);
                        break;
                    case SpecialSystemType.IntPtr:
                    case SpecialSystemType.UIntPtr:
                        m_Frame.Push(runtimeType, *(IntPtr*)data);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
            } else if (type.IsStruct) {
                if (type.IsLargeStruct) {
                    m_Frame.PushLargeStructFrom(runtimeType, data);
                } else {
                    m_Frame.Push(runtimeType, *(long*)data);
                }
            }
        }
    }
}
