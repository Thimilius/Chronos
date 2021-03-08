using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Model;
using Chronos.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Chronos.Execution {
    /// <summary>
    /// The stack frame 
    /// </summary>
    public unsafe partial class InterpreterStackFrame : IDisposable {
        /// <summary>
        /// The size of a regular stack slot.
        /// </summary>
        private const int SLOT_SIZE = 8;

        /// <summary>
        /// The method this stack frame belongs to.
        /// </summary>
        public MethodDescription Method { get; }

        /// <summary>
        /// Holds a reference to the stack allocator.
        /// </summary>
        private readonly StackAllocator m_StackAllocator;
        /// <summary>
        /// Holds a reference to the method body.
        /// </summary>
        private readonly MethodBodyDescription m_MethodBody;
        /// <summary>
        /// Holds a copy of the method call data.
        /// </summary>
        private readonly MethodCallData m_MethodCallData;

        /// <summary>
        /// The size of the stack memory.
        /// </summary>
        private readonly int m_StackSize;
        /// <summary>
        /// The memory of the stack.
        /// </summary>
        private readonly byte* m_StackMemory;
        /// <summary>
        /// The current offset into the stack memory in bytes.
        /// </summary>
        private int m_StackByteOffset;

        /// <summary>
        /// The size of the large struct stack memory.
        /// </summary>
        private int m_LargeStructStackSize;
        /// <summary>
        /// The memory of the large struct stack.
        /// </summary>
        private byte* m_LargeStructStackMemory;
        /// <summary>
        /// The current offset into the large struct memory in bytes.
        /// </summary>
        private int m_LargeStructStackByteOffset;

        /// <summary>
        /// The types of the stack items.
        /// </summary>
        private readonly RuntimeType[] m_RuntimeTypesStack;
        /// <summary>
        /// The current offset into the stack types.
        /// </summary>
        private int m_RuntimeTypesStackOffset;

        /// <summary>
        /// The memory of the local variables.
        /// </summary>
        private readonly byte* m_LocalsMemory;
        /// <summary>
        /// The info of the local varibales.
        /// </summary>
        private readonly IList<LocalVariableInfo> m_Locals;

        /// <summary>
        /// Constructs a new interpreter stack frame.
        /// </summary>
        /// <param name="stackAllocator">The stack allocator</param>
        /// <param name="method">The method the stack frame belongs to</param>
        /// <param name="callData">A reference to the method call data</param>
        public InterpreterStackFrame(StackAllocator stackAllocator, MethodDescription method, ref MethodCallData callData) {
            Method = method;

            m_StackAllocator = stackAllocator;
            m_MethodBody = method.Body;
            m_MethodCallData = callData;

            m_StackSize = GetStackSize(m_MethodBody);
            m_StackMemory = stackAllocator.Allocate(m_StackSize);
            m_StackByteOffset = 0;

            m_LargeStructStackSize = 0;
            m_LargeStructStackMemory = null;
            m_LargeStructStackByteOffset = 0;

            m_RuntimeTypesStack = new RuntimeType[m_MethodBody.MaxStack];
            m_RuntimeTypesStackOffset = 0;

            m_Locals = m_MethodBody.Locals;
            m_LocalsMemory = stackAllocator.Allocate(m_MethodBody.LocalsSize);

            InitializeLocals();
        }

        /// <summary>
        /// Empties the stack.
        /// </summary>
        public void EmptyStack() {
            m_StackByteOffset = 0;
            m_LargeStructStackByteOffset = 0;
            m_RuntimeTypesStackOffset = 0;
        }

        /// <summary>
        /// Pushes a new value on the stack.
        /// </summary>
        /// <typeparam name="T">The type of the value to push</typeparam>
        /// <param name="type">The runtime type of the value</param>
        /// <param name="value">The value to push</param>
        public void Push<T>(RuntimeType type, T value) where T : unmanaged {
            Debug.Assert(m_StackByteOffset + SLOT_SIZE <= m_StackSize);
            Debug.Assert(m_RuntimeTypesStackOffset + 1 <= m_RuntimeTypesStack.Length);

            m_RuntimeTypesStack[m_RuntimeTypesStackOffset++] = type;

            *(T*)&m_StackMemory[m_StackByteOffset] = value;
            m_StackByteOffset += SLOT_SIZE;
        }

        /// <summary>
        /// Pushes a large struct on the stack from a source.
        /// </summary>
        /// <param name="type">The runtime type of the large struct</param>
        /// <param name="source">The source to copy the large struct from</param>
        public void PushLargeStructFrom(RuntimeType type, void* source) {
            int size = type.Type.Size;

            Debug.Assert(m_RuntimeTypesStackOffset + 1 <= m_RuntimeTypesStack.Length);
            LargeStructStackEnsureCanPush(size);
            Debug.Assert(m_LargeStructStackByteOffset + size <= m_LargeStructStackSize);

            byte* destination = GetLargeValueTypeStackSlot();

            m_RuntimeTypesStack[m_RuntimeTypesStackOffset++] = type;
            m_LargeStructStackByteOffset += size;

            // We save the pointer to the large struct in the actual operand stack.
            *(byte**)&m_StackMemory[m_StackByteOffset] = destination;
            m_StackByteOffset += SLOT_SIZE;

            Unsafe.CopyBlock(destination, source, (uint)size);
        }

        /// <summary>
        /// Pushes a large struct on the stack and returns a pointer to the reserved memory.
        /// </summary>
        /// <param name="type">The runtime type of the large struct</param>
        /// <returns>The pointer to the reserved large struct memory</returns>
        public IntPtr ReserveLargeStructSpace(RuntimeType type) {
            int size = type.Type.Size;

            Debug.Assert(m_RuntimeTypesStackOffset + 1 <= m_RuntimeTypesStack.Length);
            LargeStructStackEnsureCanPush(size);
            Debug.Assert(m_LargeStructStackByteOffset + size <= m_LargeStructStackSize);

            byte* destination = GetLargeValueTypeStackSlot();

            m_RuntimeTypesStack[m_RuntimeTypesStackOffset++] = type;
            m_LargeStructStackByteOffset += size;

            // We save the pointer to the large struct in the actual operand stack.
            *(byte**)&m_StackMemory[m_StackByteOffset] = destination;
            m_StackByteOffset += SLOT_SIZE;

            return new IntPtr(destination);
        }

        /// <summary>
        /// Pops a value off the stack.
        /// </summary>
        /// <typeparam name="T">The type of the value to pop</typeparam>
        /// <returns>The popped of value</returns>
        public T Pop<T>() where T : unmanaged {
            Debug.Assert(m_StackByteOffset - SLOT_SIZE >= 0);
            Debug.Assert(m_RuntimeTypesStackOffset - 1 >= 0);

            m_RuntimeTypesStackOffset--;

            m_StackByteOffset -= SLOT_SIZE;
            return *(T*)&m_StackMemory[m_StackByteOffset];
        }

        /// <summary>
        /// Pops a large struct to a given destination.
        /// </summary>
        /// <param name="destination">The destination to pop the large struct to</param>
        public void PopLargeStructTo(void* destination) {
            Debug.Assert(m_RuntimeTypesStackOffset - 1 >= 0);

            m_RuntimeTypesStackOffset--;
            TypeDescription type = m_RuntimeTypesStack[m_RuntimeTypesStackOffset].Type;
            Debug.Assert(type.IsLargeStruct);
            int size = type.Size;

            m_LargeStructStackByteOffset -= size;
            m_StackByteOffset -= SLOT_SIZE;

            void* source = GetLargeValueTypeStackSlot();

            Unsafe.CopyBlock(destination, source, (uint)size);
        }

        /// <summary>
        /// Pops a large struct without copying it to a location.
        /// </summary>
        public void PopLargeStruct() {
            Debug.Assert(m_RuntimeTypesStackOffset - 1 >= 0);

            m_RuntimeTypesStackOffset--;
            TypeDescription type = m_RuntimeTypesStack[m_RuntimeTypesStackOffset].Type;
            Debug.Assert(type.IsLargeStruct);
            m_LargeStructStackByteOffset -= type.Size;
            m_StackByteOffset -= SLOT_SIZE;
        }

        /// <summary>
        /// Peeks at a stack value with a given offset.
        /// </summary>
        /// <typeparam name="T">The type of the stack item</typeparam>
        /// <param name="offset">The offset to peek into</param>
        /// <returns>The stack value corresponding to the offset</returns>
        public T Peek<T>(int offset) where T : unmanaged {
            Debug.Assert(m_StackByteOffset - SLOT_SIZE >= 0);

            return *(T*)&m_StackMemory[m_StackByteOffset - (SLOT_SIZE * (offset + 1))];
        }

        /// <summary>
        /// Peeks at a stack type with a given offset.
        /// </summary>
        /// <param name="offset">The offset to peek into</param>
        /// <returns>The stack type corresponding to the offset</returns>
        public RuntimeType PeekType(int offset) {
            Debug.Assert(offset >= 0);
            Debug.Assert(m_RuntimeTypesStackOffset - 1 - offset >= 0);

            return m_RuntimeTypesStack[m_RuntimeTypesStackOffset - 1 - offset];
        }

        /// <summary>
        /// Peeks at a stack item type with a given offset.
        /// </summary>
        /// <param name="offset">The offset to peek into</param>
        /// <returns>The stack item type corresponding to the offset</returns>
        public StackItemType PeekItemType(int offset) {
            Debug.Assert(offset >= 0);
            Debug.Assert(m_RuntimeTypesStackOffset - 1 - offset >= 0);

            return m_RuntimeTypesStack[m_RuntimeTypesStackOffset - 1 - offset].ItemType;
        }

        /// <summary>
        /// Peeks at the address of a stack item at a given offset.
        /// </summary>
        /// <param name="offset">The offset to peek into</param>
        /// <returns>The address of stack item corresponding to the offset</returns>
        public IntPtr PeekAddress(int offset) {
            return new IntPtr(&m_StackMemory[m_StackByteOffset - (SLOT_SIZE * (offset + 1))]);
        }

        /// <summary>
        /// Peeks at the address of the current large struct on top of the stack.
        /// </summary>
        /// <returns>The address of the current large struct on top of the stack</returns>
        public IntPtr PeekLargeStructAddress() {
            TypeDescription type = m_RuntimeTypesStack[m_RuntimeTypesStackOffset - 1].Type;

            Debug.Assert(type.IsLargeStruct);
            Debug.Assert(m_LargeStructStackByteOffset - type.Size >= 0);

            return new IntPtr(&m_LargeStructStackMemory[m_LargeStructStackByteOffset - type.Size]);
        }

        /// <summary>
        /// Sets a stack item value at a given offset.
        /// </summary>
        /// <param name="offset">The offset to set the value</param>
        /// <param name="type">The runtime type of the value</param>
        /// <param name="value">The value to push</param>
        public void Set(int offset, RuntimeType type, IntPtr value) {
            m_RuntimeTypesStack[m_RuntimeTypesStackOffset - 1 - offset] = type;

            *(IntPtr*)&m_StackMemory[m_StackByteOffset - (SLOT_SIZE * (offset + 1))] = value;
        }

        /// <summary>
        /// Loads a local variable at a given index onto the stack.
        /// </summary>
        /// <param name="index">The index of the local variable to load</param>
        public void LoadLocal(int index) {
            Debug.Assert(index >= 0 && index < m_Locals.Count);

            RuntimeType localRuntimeType = m_Locals[index].RuntimeType;
            int localOffset = m_Locals[index].Offset;
            void* localMemory = &m_LocalsMemory[localOffset];

            if (localRuntimeType.Type.IsLargeStruct) {
                PushLargeStructFrom(localRuntimeType, localMemory);
            } else {
                Push(localRuntimeType, *(long*)localMemory);
            }
        }

        /// <summary>
        /// Loads the address of a local variable at a given address.
        /// </summary>
        /// <param name="index">The index of the local variable address to load</param>
        public void LoadLocalAddress(int index) {
            Debug.Assert(index >= 0 && index < m_Locals.Count);

            RuntimeType localRuntimeType = m_Locals[index].RuntimeType;
            int localOffset = m_Locals[index].Offset;

            // For large value types we do not need a special case as the address calculation is the same

            Push(RuntimeType.FromReference(localRuntimeType.Type), new IntPtr(&m_LocalsMemory[localOffset]));
        }

        /// <summary>
        /// Stores the current top stack item into a local variable.
        /// </summary>
        /// <param name="index">The index of the local variable to store the stack item into</param>
        public void StoreLocal(int index) {
            Debug.Assert(index >= 0 && index < m_Locals.Count);

            RuntimeType localRuntimeType = m_Locals[index].RuntimeType;
            int localOffset = m_Locals[index].Offset;
            void* localMemory = &m_LocalsMemory[localOffset];

            if (localRuntimeType.Type.IsLargeStruct) {
                PopLargeStructTo(localMemory);
            } else {
                *(long*)&m_LocalsMemory[localOffset] = Pop<long>();
            }
        }

        /// <summary>
        /// Traces the current stack values.
        /// </summary>
        /// <param name="tracer">The tracer to use</param>
        public void TraceStack(ITracer tracer) {
            static string PointerToString(IntPtr pointer) {
                if (pointer != IntPtr.Zero) {
                    return $"0x{pointer.ToString("X")}";
                } else {
                    return "null";
                }
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("[");
            for (int i = 0; i < m_RuntimeTypesStackOffset; i++) {
                RuntimeType runtimeType = m_RuntimeTypesStack[i];
                StackItemType stackItemType = runtimeType.ItemType;

                int byteOffset = i * SLOT_SIZE;

                if (i > 0) {
                    builder.Append(", ");
                }

                builder.Append($"(");
                builder.Append(stackItemType.ToString());
                builder.Append(": ");

                switch (stackItemType) {
                    case StackItemType.Int32:
                        builder.Append(PeekAt<int>(byteOffset));
                        break;
                    case StackItemType.Int64:
                        builder.Append(PeekAt<long>(byteOffset));
                        break;
                    case StackItemType.NativeInt:
                        builder.Append(PeekAt<IntPtr>(byteOffset));
                        break;
                    case StackItemType.Double:
                        builder.Append(PeekAt<double>(byteOffset).ToString("0.####"));
                        break;
                    case StackItemType.ByReference:
                        builder.Append(PointerToString(PeekAt<IntPtr>(byteOffset)));
                        builder.Append("(");
                        builder.Append(runtimeType.Type);
                        builder.Append(")");
                        break;
                    case StackItemType.ObjectReference:
                        IntPtr pointer = PeekAt<IntPtr>(byteOffset);
                        TypeDescription objectType = null;
                        if (pointer != IntPtr.Zero) {
                            objectType = ((ObjectBase*)pointer)->Type;
                        }
                        builder.Append(PointerToString(pointer));
                        builder.Append("(");
                        builder.Append(objectType);
                        builder.Append(")");
                        break;
                    case StackItemType.ValueType:
                        builder.Append(runtimeType.Type.Size);
                        break;
                    case StackItemType.None:
                    default:
                        Debug.Assert(false);
                        break;
                }

                builder.Append(")");
            }
            builder.Append("]");

            tracer.TraceColorLine(TracingConfig.STACK_COLOR, builder.ToString());
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() {
            m_StackAllocator.Free(m_LocalsMemory);
            m_StackAllocator.Free(m_StackMemory);

            Marshal.FreeHGlobal(new IntPtr(m_LargeStructStackMemory));
        }

        /// <summary>
        /// Ensures that the large struct stack has enough space to push more structs into it.
        /// </summary>
        /// <param name="size">The size that is needed</param>
        private void LargeStructStackEnsureCanPush(int size) {
            int remaining = m_LargeStructStackSize - size;
            if (remaining < size) {
                int newSize = Math.Max(m_LargeStructStackSize + size * 4, m_LargeStructStackSize * 2);
                byte* newStack = (byte*)Marshal.AllocHGlobal(newSize);
                m_LargeStructStackSize = newSize;

                // Initialize the memory to 0.
                Unsafe.InitBlock(newStack, 0, (uint)newSize);

                // If we have an old large struct stack, we have to copy it to the new one.
                if (m_LargeStructStackMemory != null) {
                    Unsafe.CopyBlock(newStack, m_LargeStructStackMemory, (uint)m_LargeStructStackByteOffset);
                    Marshal.FreeHGlobal(new IntPtr(m_LargeStructStackMemory));
                }
                m_LargeStructStackMemory = newStack;
            }
        }

        /// <summary>
        /// Peeks at a value at a given byte offset.
        /// </summary>
        /// <typeparam name="T">The type of the value to peek</typeparam>
        /// <param name="byteOffset">The byte offset to peek into</param>
        /// <returns>The value corresponding to the byte offset</returns>
        private T PeekAt<T>(int byteOffset) where T : unmanaged {
            return *(T*)(m_StackMemory + byteOffset);
        }

        /// <summary>
        /// Initializes the local variables.
        /// </summary>
        private void InitializeLocals() {
            // Initialize local variables to zero if required
            if (m_MethodBody.InitLocals) {
                Unsafe.InitBlock(m_LocalsMemory, 0, (uint)m_MethodBody.LocalsSize);
            }
        }

        /// <summary>
        /// Gets the current large struct stack slot.
        /// </summary>
        /// <returns>The pointer to the current large struct stack slot</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte* GetLargeValueTypeStackSlot() {
            return &m_LargeStructStackMemory[m_LargeStructStackByteOffset];
        }

        /// <summary>
        /// Gets the size in bytes of the stack for a given method body.
        /// </summary>
        /// <param name="body">The method body</param>
        /// <returns>The size in bytes of the stack for the method body</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetStackSize(MethodBodyDescription body) {
            return body.MaxStack * SLOT_SIZE;
        }
    }
}
