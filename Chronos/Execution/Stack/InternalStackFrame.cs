using Chronos.Memory;
using Chronos.Metadata;

namespace Chronos.Execution {
    /// <summary>
    /// Represents a stack frame for an internally implemented method.
    /// </summary>
    public class InternalStackFrame : IStackFrame {
        /// <summary>
        /// <inheritdoc cref="IStackFrame.Method"/>
        /// </summary>
        public MethodDescription Method { get; }

        /// <summary>
        /// Constructs a new internal stack frame.
        /// </summary>
        /// <param name="method">The method of the stack frame</param>
        public InternalStackFrame(MethodDescription method) {
            Method = method;
        }

        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectStack(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectStack(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) { }
        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectLocals(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectLocals(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) { }
        /// <summary>
        /// <inheritdoc cref="IStackFrame.InspectArguments(ObjectReferenceCallback, ByReferenceCallback)"/>
        /// </summary>
        public void InspectArguments(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback) { }
    }
}
