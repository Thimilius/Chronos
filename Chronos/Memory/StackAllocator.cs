using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chronos.Memory {
    /// <summary>
    /// Allocates memory with a stack semantic
    /// </summary>
    public unsafe class StackAllocator : IAllocator, IDisposable {
        /// <summary>
        /// Saves the sizes of all allocated stack frames.
        /// </summary>
        // NOTE: A real stack would have a 'base' and 'stack' pointer
        //       and would save the size of a frame in the memory itself.
        private readonly Stack<int> m_Allocations;
        /// <summary>
        /// Holds the size of the allocated stack memory.
        /// </summary>
        private readonly int m_MemorySize;
        /// <summary>
        /// Handle to the allocated stack memory.
        /// </summary>
        private readonly byte* m_Memory;

        /// <summary>
        /// Gets the current stack offset.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Creates a new stack allocator with a given stack size.
        /// </summary>
        /// <param name="initalSize">The inital stack size in bytes</param>
        public StackAllocator(int initalSize) {
            Debug.Assert(initalSize > 0);

            m_Allocations = new Stack<int>();

            m_MemorySize = initalSize;
            Offset = 0;
            m_Memory = (byte*)Marshal.AllocHGlobal(initalSize).ToPointer();
        }

        /// <summary>
        /// <inheritdoc cref="IAllocator.Allocate(int)"/>
        /// </summary>
        public byte* Allocate(int size) {
            Debug.Assert(size >= 0);

            if (size <= 0) {
                return null;
            }

            m_Allocations.Push(size);

            if (Offset + size > m_MemorySize) {
                throw new StackOverflowException();
            }

            byte* p = m_Memory + Offset;

            Offset += size;

            return p;
        }

        /// <summary>
        /// <inheritdoc cref="IAllocator.Free(byte*)"/>
        /// </summary>
        public void Free(byte* pointer) {
            if (pointer == null) {
                return;
            }

            Debug.Assert(m_Allocations.Count != 0);

            Offset -= m_Allocations.Pop();
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() {
            Marshal.FreeHGlobal(new IntPtr(m_Memory));
        }
    }
}
