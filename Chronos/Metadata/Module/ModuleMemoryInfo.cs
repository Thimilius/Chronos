namespace Chronos.Metadata {
    /// <summary>
    /// Contains memory information of a module.
    /// </summary>
    public struct ModuleMemoryInfo {
        /// <summary>
        /// The size of the stack.
        /// </summary>
        public int StackSize { get; }
        /// <summary>
        /// The size of the heap.
        /// </summary>
        public int HeapSize { get; }

        /// <summary>
        /// Constructs a new module memory info-
        /// </summary>
        /// <param name="stackSize">The size of the stack</param>
        /// <param name="heapSize">The size of the heap</param>
        public ModuleMemoryInfo(int stackSize, int heapSize) {
            StackSize = stackSize;
            HeapSize = heapSize;
        }
    }
}
