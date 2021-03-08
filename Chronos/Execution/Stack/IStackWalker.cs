namespace Chronos.Execution {
    /// <summary>
    /// Interface for walking the call stack.
    /// </summary>
    public interface IStackWalker {
        /// <summary>
        /// Function that gets called for every encountered stack frame.
        /// </summary>
        /// <param name="frame">The stack frame that got encountered</param>
        void OnStackFrame(IStackFrame frame);
    }
}
