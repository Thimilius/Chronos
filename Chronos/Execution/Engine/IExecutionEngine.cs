using Chronos.Metadata;
using Chronos.Model;
using System.Collections.Generic;

namespace Chronos.Execution {
    /// <summary>
    /// Interface for an execution engine.
    /// </summary>
    public interface IExecutionEngine {
        /// <summary>
        /// The size of a stack slot.
        /// </summary>
        int StackSlotSize { get; }
        /// <summary>
        /// The current call stack.
        /// </summary>
        IEnumerable<IStackFrame> CallStack { get; }

        /// <summary>
        /// Makes a method call.
        /// </summary>
        /// <param name="method">The method to call</param>
        /// <param name="callData">The method call data</param>
        void MakeCall(MethodDescription method, ref MethodCallData callData);
        /// <summary>
        /// Ensures that the static constructor for a type has been run.
        /// </summary>
        /// <param name="type">The type to ensure the static constructor has run for</param>
        void EnsureStaticConstructorHasRun(TypeDescription type);
        /// <summary>
        /// Runs the finalizer of the given object.
        /// </summary>
        /// <param name="obj">The object to run the finalizer for</param>
        unsafe void RunFinalizer(ObjectBase* obj);
        /// <summary>
        /// Walks the current execution stack.
        /// </summary>
        /// <param name="stackWalker">The object used for walking the stack</param>
        void WalkStack(IStackWalker stackWalker);
    }
}
