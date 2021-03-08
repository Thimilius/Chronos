namespace Chronos.Execution {
    /// <summary>
    /// Represents a local variable.
    /// </summary>
    public struct LocalVariableInfo {
        public RuntimeType RuntimeType { get; set; }
        public int Offset { get; set; }
    }
}
