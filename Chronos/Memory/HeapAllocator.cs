using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Chronos.Memory {
    /// <summary>
    /// A general purpose allocator.
    /// 
    /// Currently this implementation is just a wrapper to allocate and free from the native heap.
    /// For better performance it is probably best we implement our own general purpose allocator.
    /// Something similar to this: http://dmitrysoshnikov.com/compilers/writing-a-memory-allocator/
    /// </summary>
    public unsafe class HeapAllocator : IAllocator, IDisposable {
        /// <summary>
        /// Creates a new stack allocator with a given heap size.
        /// </summary>
        /// <param name="heapSize">The initial heap size in bytes</param>
        public HeapAllocator(int heapSize) {
            // NOTE: Because of our simple implementation the heap size gets ignored
        }

        /// <summary>
        /// <inheritdoc cref="IAllocator.Allocate(int)"/>
        /// </summary>
        public byte* Allocate(int size) {
            Debug.Assert(size >= 0);

            if (size <= 0) {
                return null;
            }

            return (byte*)Marshal.AllocHGlobal(size).ToPointer();
        }

        /// <summary>
        /// <inheritdoc cref="IAllocator.Free(byte*)"/>
        /// </summary>
        public void Free(byte* pointer) {
            Marshal.FreeHGlobal(new IntPtr(pointer));
        }

        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() {
            // We do not need to do anything here
        }
    }
}
