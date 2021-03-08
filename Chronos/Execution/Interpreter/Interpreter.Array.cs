using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Creates a new zero-based single-dimensional array of a type and size.
        /// 
        /// Stack: ..., number of elements -> ..., array
        /// </summary>
        /// <param name="token">The token of the type</param>
        private unsafe void InterpretNewArray(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);
            ArrayTypeDescription arrayType = MetadataSystem.GetArrayType(type, 1);

            int length = 0;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                length = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                length = (int)m_Frame.Pop<IntPtr>();
            } else {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            ArrayObject* array = VirtualMachine.GarbageCollector.AllocateNewSZArray(arrayType, length);

            m_Frame.Push(RuntimeType.FromObjectReference(arrayType), (IntPtr)array);
        }

        /// <summary>
        /// Pushes the length of an array on the stack.
        /// 
        /// Stack: ..., array -> ..., length 
        /// </summary>
        private unsafe void InterpretLoadArrayLength() {
            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();

            ThrowOnInvalidPointer(array);

            m_Frame.Push(RuntimeType.NativeInt, new IntPtr(array->Length));
        }

        /// <summary>
        /// Pushes an element of an array on the stack.
        /// 
        /// Stack: ..., array, index -> ..., value
        /// </summary>
        /// <typeparam name="TFrom">The type of the array element</typeparam>
        /// <typeparam name="TTo">The type the array element should be cast to</typeparam>
        /// <param name="runtimeType">The runtime type to push</param>
        private unsafe void InterpretLoadArrayElement<TFrom, TTo>(RuntimeType runtimeType) where TFrom : unmanaged where TTo : unmanaged {
            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            TFrom* data = ObjectModel.GetArrayBuffer<TFrom>(array);
            m_Frame.Push(runtimeType, Unsafe.As<TFrom, TTo>(ref data[index]));
        }

        /// <summary>
        /// Pushes a float element of an array on the stack.
        /// 
        /// Stack: ..., array, index -> ..., value
        /// </summary>
        private unsafe void InterpretLoadArrayElementFloat() {
            // NOTE: This is a direct copy of InterpretLoadArrayElement<T> but we need the special float to double conversion.

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            float* data = ObjectModel.GetArrayBuffer<float>(array);
            m_Frame.Push(RuntimeType.Double, (double)data[index]);
        }

        /// <summary>
        /// Pushes a reference element of an array on the stack.
        /// 
        /// Stack: ..., array, index -> ..., value
        /// </summary>
        private unsafe void InterpretLoadArrayElementReference() {
            // NOTE: This is a direct copy of InterpretLoadArrayElement<T>.

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            IntPtr* data = ObjectModel.GetArrayBuffer<IntPtr>(array);
            IntPtr pointer = data[index];

            // For references we need to find the actual type of the object,
            // so that it can be tracked properly.
            TypeDescription type = ((ObjectBase*)pointer)->Type;

            m_Frame.Push(RuntimeType.FromObjectReference(type), pointer);
        }

        /// <summary>
        /// Pushes an element of an array of a certain type on the stack.
        /// 
        /// Stack: ..., array, index -> ..., value
        /// </summary>
        /// <param name="token">The type token of the array element</param>
        private unsafe void InterpretLoadArrayElement(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);
            int size = type.Size;

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            IntPtr* data = ObjectModel.GetArrayBuffer<IntPtr>(array);

            if (type.IsStruct) {
                if (type.IsLargeStruct) {
                    m_Frame.PushLargeStructFrom(RuntimeType.FromType(type), data);
                } else {
                    m_Frame.Push(RuntimeType.FromType(type), *(long*)data);
                }
            } else {
                switch (size) {
                    case 1:
                        m_Frame.Push(RuntimeType.FromType(type), *(byte*)data);
                        break;
                    case 2:
                        m_Frame.Push(RuntimeType.FromType(type), *(short*)data);
                        break;
                    case 4:
                        m_Frame.Push(RuntimeType.FromType(type), *(int*)data);
                        break;
                    case 8:
                        m_Frame.Push(RuntimeType.FromType(type), *(long*)data);
                        break;
                }
            }
        }

        /// <summary>
        /// Pushes a the address of an element of an array on the stack.
        /// 
        /// Stack: ..., array, index -> ..., address (managed)
        /// </summary>
        /// <param name="token">The type token of the array element</param>
        private unsafe void InterpretLoadArrayElementAddress(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription elementType = m_TokenResolver.ResolveType(token);

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            byte* address = ObjectModel.GetArrayBuffer<byte>(array);

            // We do not get the actual size of the type.
            // Instead we request the size a variable of the type would have.
            address += elementType.GetVariableSize() * index;

            m_Frame.Push(RuntimeType.FromReference(elementType), new IntPtr(address));
        }

        /// <summary>
        /// Stores a value from the stack to an element of an array.
        /// 
        /// Stack: ..., array, index, value -> ...
        /// </summary>
        /// <typeparam name="TFrom">The type of the array element</typeparam>
        /// <typeparam name="TTo">The type the array element should be cast to</typeparam>
        private unsafe void InterpretStoreArrayElement<TFrom, TTo>() where TFrom : unmanaged where TTo : unmanaged {
            TFrom value = m_Frame.Pop<TFrom>();

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            TTo* data = ObjectModel.GetArrayBuffer<TTo>(array);
            data[index] = Unsafe.As<TFrom, TTo>(ref value);
        }

        /// <summary>
        /// Stores a float value from the stack to an element of an array.
        /// 
        /// Stack: ..., array, index, value -> ...
        /// </summary>
        private unsafe void InterpretStoreArrayElementFloat() {
            // NOTE: This is a direct copy of InterpretStoreArrayElement<T> but we need the special double to float conversion.

            float value = (float)m_Frame.Pop<double>();

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            float* data = ObjectModel.GetArrayBuffer<float>(array);
            data[index] = value;
        }

        /// <summary>
        /// Stores a reference value from the stack to an element of an array.
        /// 
        /// Stack: ..., array, index, value -> ...
        /// </summary>
        private unsafe void InterpretStoreArrayElementReference() {
            // NOTE: This is a direct copy of InterpretStoreArrayElement<T> .

            IntPtr value = m_Frame.Pop<IntPtr>();

            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(0);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Pop<IntPtr>();
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            // NOTE: We currently do not check if the type of the reference value on the stack
            // is actually compatible with the element type of the array.
            // This is the only thing that would be different from a call to InterpretStoreArrayElement<IntPtr>.

            IntPtr* data = ObjectModel.GetArrayBuffer<IntPtr>(array);
            data[index] = value;
        }

        /// <summary>
        /// Stores an array element of a certain type.
        /// 
        /// Stack: ..., array, index, value -> ...
        /// </summary>
        /// <param name="token">The type token of the array element</param>
        private unsafe void InterpretStoreArrayElement(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);

            TypeDescription type = m_TokenResolver.ResolveType(token);
            int size = type.Size;

            if (type.IsStruct) {
                if (type.IsLargeStruct) {
                    StoreArrayElementLargeStruct(type);
                } else {
                    InterpretStoreArrayElement<long, long>();
                }
            } else {
                switch (size) {
                    case 1:
                        InterpretStoreArrayElement<byte, byte>();
                        break;
                    case 2:
                        InterpretStoreArrayElement<short, short>();
                        break;
                    case 4:
                        InterpretStoreArrayElement<int, int>();
                        break;
                    case 8:
                        InterpretStoreArrayElement<long, long>();
                        break;
                }
            }
        }

        /// <summary>
        /// Stores a large struct array element-
        /// </summary>
        /// <param name="elementType">The type of the array element</param>
        private unsafe void StoreArrayElementLargeStruct(TypeDescription elementType) {
            int index;
            StackItemType stackItemType = m_Frame.PeekItemType(1);
            if (stackItemType == StackItemType.Int32) {
                index = m_Frame.Peek<int>(1);
            } else if (stackItemType == StackItemType.NativeInt) {
                index = (int)m_Frame.Peek<IntPtr>(1);
            } else {
                throw new InvalidProgramException();
            }

            ArrayObject* array = (ArrayObject*)m_Frame.Peek<IntPtr>(2);
            ThrowOnInvalidPointer(array);

            // Make sure the index is in range.
            int length = array->Length;
            if (index < 0 || index >= length) {
                VirtualMachine.ExceptionEngine.ThrowIndexOutOfRangeException();
            }

            byte* address = ((byte*)array) + ArrayObject.BUFFER_OFFSET;
            // We do not get the actual size of the type.
            // Instead we request the size a variable of the type would have.
            address += (elementType.GetVariableSize() * index);

            m_Frame.PopLargeStructTo(address);

            // Now we do the actual pop of the index and array object.
            if (stackItemType == StackItemType.Int32) {
                m_Frame.Pop<int>();
            } else if (stackItemType == StackItemType.NativeInt) {
                m_Frame.Pop<IntPtr>();
            } else {
                throw new InvalidProgramException();
            }
            m_Frame.Pop<IntPtr>();
        }
    }
}
