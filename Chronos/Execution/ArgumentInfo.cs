namespace Chronos.Execution {
    /// <summary>
    /// Represents an argument type when a function is called.
    /// </summary>
    public struct ArgumentInfo {
        /// <summary>
        /// The runtime type of the argument.
        /// </summary>
        public RuntimeType RuntimeType { get; set; }
        /// <summary>
        /// The offset where the argument is stored in memory.
        /// </summary>
        public int Offset { get; set; }
    }
}
