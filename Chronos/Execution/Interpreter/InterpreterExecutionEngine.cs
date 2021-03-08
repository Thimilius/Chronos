using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Tracing;
using System.Diagnostics;

namespace Chronos.Execution {
    /// <summary>
    /// Execution engine that uses a bytecode interpreter.
    /// </summary>
    public class InterpreterExecutionEngine : ExecutionEngine {
        /// <summary>
        /// <inheritdoc cref="ExecutionEngine.StackSlotSize"/>
        /// </summary>
        public override int StackSlotSize => 8;
        /// <summary>
        /// <inheritdoc cref="ExecutionEngine.Tracer"/>
        /// </summary>
        protected override ITracer Tracer { get; }
        /// <summary>
        /// <inheritdoc cref="ExecutionEngine.StackAllocator"/>
        /// </summary>
        protected override StackAllocator StackAllocator { get; }

        /// <summary>
        /// Constructs a new interpreter execution engine.
        /// </summary>
        /// <param name="tracer">The tracer to use</param>
        /// <param name="stackSize">The size of the stack</param>
        public InterpreterExecutionEngine(ITracer tracer, int stackSize) {
            Debug.Assert(tracer != null);

            Tracer = tracer;
            StackAllocator = new StackAllocator(stackSize);
        }

        /// <summary>
        /// <inheritdoc cref="ExecutionEngine.MakeManagedCall(MethodDescription, ref MethodCallData)"/>
        /// </summary>
        protected override void MakeManagedCall(MethodDescription method, ref MethodCallData callData) {
            using Interpreter interpreter = new Interpreter(Tracer, StackAllocator, method, ref callData);
            
            try {
                PushStackFrame(interpreter.StackFrame);
                interpreter.InterpretMethod(ref callData);
            } finally {
                PopStackFrame();
            }
        }
    }
}
