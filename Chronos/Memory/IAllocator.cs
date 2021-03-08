using System;

namespace Chronos.Memory {
    /// <summary>
    /// Interface for allocating and freeing memory.
    /// </summary>
    public unsafe interface IAllocator : IDisposable {
        /// <summary>
        /// Allocates a new block with a given size.
        /// </summary>
        /// <param name="size">The size of the block to allocate in bytes</param>
        /// <returns>The new allocated block</returns>
        byte* Allocate(int size);
        /// <summary>
        /// Frees a given block.
        /// </summary>
        /// <param name="pointer">The memory block to free</param>
        void Free(byte* pointer);
    }
}
