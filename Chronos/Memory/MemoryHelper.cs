namespace Chronos.Memory {
    /// <summary>
    /// Helper class for memory operations.
    /// </summary>
    public static unsafe class MemoryHelper {
        /// <summary>
        /// Checks if the memory at two locations are the same.
        /// </summary>
        /// <param name="a">The first memory location</param>
        /// <param name="b">The second memory location</param>
        /// <param name="size">The size of the memory to compare</param>
        /// <returns>True if the memory is the same otherwise false</returns>
        public static bool MemoryCompare(byte* a, byte* b, int size) {
            for (int i = 0; i < size; i++) {
                if (a[i] != b[i]) {
                    return false;
                }
            }

            return true;
        }
    }
}
