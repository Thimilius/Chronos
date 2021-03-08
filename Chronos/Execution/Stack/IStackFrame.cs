using Chronos.Memory;
using Chronos.Metadata;

namespace Chronos.Execution {
    /// <summary>
    /// Interface that represents a single stack frame.
    /// </summary>
    public interface IStackFrame {
        /// <summary>
        /// The method that the stack frame belongs to.
        /// </summary>
        MethodDescription Method { get; }

        /// <summary>
        /// Inspects the operand stack for object references.
        /// </summary>
        /// <param name="objectReferenceCallback">Callback that gets called when an object reference is found</param>
        /// <param name="byReferenceCallback">Callback that gets called when a by reference is found</param>
        void InspectStack(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback);
        /// <summary>
        /// Inspects the local variables for object references.
        /// </summary>
        /// <param name="objectReferenceCallback">Callback that gets called when an object reference is found</param>
        /// <param name="byReferenceCallback">Callback that gets called when a by reference is found</param>
        void InspectLocals(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback);
        /// <summary>
        /// Inspects the function arguments for object references.
        /// </summary>
        /// <param name="objectReferenceCallback">Callback that gets called when an object reference is found</param>
        /// <param name="byReferenceCallback">Callback that gets called when a by reference is found</param>
        void InspectArguments(ObjectReferenceCallback objectReferenceCallback, ByReferenceCallback byReferenceCallback);
    }
}
