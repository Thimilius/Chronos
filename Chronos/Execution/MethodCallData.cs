using Chronos.Memory;
using Chronos.Metadata;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    /// <summary>
    /// Stores data for method calls.
    /// </summary>
    public unsafe struct MethodCallData : IDisposable {
        /// <summary>
        /// The size of a regular argument or return value slot.
        /// </summary>
        public const int SLOT_SIZE = 8;

        /// <summary>
        /// The number of arguments stored (including a possible 'this' pointer).
        /// </summary>
        public int ArgumentCount { get; }
        /// <summary>
        /// The size of the arguments memory.
        /// </summary>
        public int ArgumentSize { get; }
        /// <summary>
        /// The size of the return memory.
        /// </summary>
        public int ReturnSize { get; }

        /// <summary>
        /// The memory where arguments are stored in.
        /// </summary>
        private readonly byte* m_ArgumentsMemory;

        /// <summary>
        /// Holds a reference to the stack allocator.
        /// </summary>
        private readonly StackAllocator m_StackAllocator;
        
        /// <summary>
        /// The types of all arguments (including a possible 'this' pointer).
        /// </summary>
        private readonly ArgumentInfo[] m_ArgumentTypes;

        /// <summary>
        /// The memory where the return value is stored in.
        /// </summary>
        private readonly byte* m_ReturnMemory;
        /// <summary>
        /// The type of the return value.
        /// </summary>
        private RuntimeType m_ReturnType;

        /// <summary>
        /// Constructs a new method call data.
        /// </summary>
        /// <param name="stackAllocator">The stackallocator</param>
        /// <param name="method">The method to call</param>
        /// <param name="argumentTypes">The types of all arguments</param>
        /// <param name="argumentSize">The required size for all arguments</param>
        /// <param name="returnTypeOverwrite">An optional return type that should be set</param>
        public MethodCallData(StackAllocator stackAllocator, MethodDescription method, ArgumentInfo[] argumentTypes, int argumentSize, TypeDescription returnTypeOverwrite = null) {
            Debug.Assert(stackAllocator != null);

            m_StackAllocator = stackAllocator;

            // Setup a return type stub only used to calculate the size of the return memory.
            // The actual proper return type if present gets overwritten when the return value is set.
            m_ReturnType = RuntimeType.FromType(returnTypeOverwrite ?? method.Signature.ReturnType);
            ReturnSize = 0;
            if (m_ReturnType.Type.IsLargeStruct) {
                ReturnSize = m_ReturnType.Type.Size;
            } else if (!m_ReturnType.Type.IsVoid) {
                ReturnSize = SLOT_SIZE;
            }
            m_ReturnMemory = stackAllocator.Allocate(ReturnSize);

            ArgumentSize = argumentSize;
            m_ArgumentTypes = argumentTypes;
            ArgumentCount = argumentTypes == null ? 0 : argumentTypes.Length;
            m_ArgumentsMemory = stackAllocator.Allocate(ArgumentSize);

            // Clear memory to 0.
            Unsafe.InitBlock(m_ReturnMemory, 0, (uint)ReturnSize);
            Unsafe.InitBlock(m_ArgumentsMemory, 0, (uint)ArgumentSize);
        }

        /// <summary>
        /// Gets the value of an argument at an index.
        /// </summary>
        /// <typeparam name="T">The type of the argument value</typeparam>
        /// <param name="index">The index of the argument</param>
        /// <returns>The value of the argument</returns>
        public T GetArgument<T>(int index) where T : unmanaged {
            Debug.Assert(index >= 0 && index < ArgumentCount);

            return *(T*)&m_ArgumentsMemory[m_ArgumentTypes[index].Offset];
        }

        /// <summary>
        /// Gets the address of an argument at an index.
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <returns>The address of the argument</returns>
        public IntPtr GetArgumentAddress(int index) {
            Debug.Assert(index >= 0 && index < ArgumentCount);

            return new IntPtr(&m_ArgumentsMemory[m_ArgumentTypes[index].Offset]);
        }

        /// <summary>
        /// Gets the type of an argument at an index.
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <returns>The type of the argument</returns>
        public RuntimeType GetArgumentType(int index) {
            Debug.Assert(index >= 0 && index < ArgumentCount);

            return m_ArgumentTypes[index].RuntimeType;
        }

        /// <summary>
        /// Gets the info of an argument at an index.
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <returns>The type of the argument</returns>
        public ArgumentInfo GetArgumentInfo(int index) {
            Debug.Assert(index >= 0 && index < ArgumentCount);
            
            return m_ArgumentTypes[index];
        }

        /// <summary>
        /// Sets the value of an argument at an index.
        /// </summary>
        /// <typeparam name="T">The type of the argument value</typeparam>
        /// <param name="index">The index of the argument</param>
        /// <param name="value">The value of the argument</param>
        public void SetArgument<T>(int index, T value) where T : unmanaged {
            *(T*)&m_ArgumentsMemory[m_ArgumentTypes[index].Offset] = value;
        }

        /// <summary>
        /// Sets a large struct argument at an index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public void* SetLargeStructArgument(int index) {
            void* slot = &m_ArgumentsMemory[m_ArgumentTypes[index].Offset];
            return slot;
        }

        /// <summary>
        /// Sets the argument type at an index.
        /// </summary>
        /// <param name="index">The index of the argument</param>
        /// <param name="type">The type of the argument</param>
        public void SetArgumentType(int index, ArgumentInfo type) {
            m_ArgumentTypes[index] = type;
        }

        /// <summary>
        /// Gets the return value.
        /// </summary>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <returns>The return value</returns>
        public T GetReturnValue<T>() where T : unmanaged {
            return *(T*)m_ReturnMemory;
        }

        /// <summary>
        /// Gets the address of the return value.
        /// </summary>
        /// <returns>The address of the return value</returns>
        public void* GetReturnValueAddress() {
            return m_ReturnMemory;
        }

        /// <summary>
        /// Gets the runtime type of the return value.
        /// </summary>
        /// <returns>The runtime type of the return value</returns>
        public RuntimeType GetReturnType() {
            return m_ReturnType;
        }

        /// <summary>
        /// Sets the return value.
        /// </summary>
        /// <typeparam name="T">The type of the return value</typeparam>
        /// <param name="runtimeType">The runtime type of the return value</param>
        /// <param name="value">The return value</param>
        public void SetReturnValue<T>(RuntimeType runtimeType, T value) where T : unmanaged {
            m_ReturnType = runtimeType;
            *(T*)m_ReturnMemory = value;
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() {
            m_StackAllocator.Free(m_ArgumentsMemory);
            m_StackAllocator.Free(m_ReturnMemory);
        }
    }
}
