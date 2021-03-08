using System;

namespace Chronos.Tracing {
    /// <summary>
    /// Specifies the tracing mode.
    /// </summary>
    [Flags]
    public enum TracingMode {
        /// <summary>
        /// No tracing.
        /// </summary>
        None                   = 0,

        /// <summary>
        /// Traces the diagnosis of the execution.
        /// </summary>
        Diagnostic             = 1 << 0,
        /// <summary>
        /// Traces the execution including CIL instrunctions and operand stack.
        /// </summary>
        Execution              = 1 << 1,
        /// <summary>
        /// Traces the garbage collector.
        /// </summary>
        GC                     = 1 << 2,

        /// <summary>
        /// Traces both diagnosis and execution.
        /// </summary>
        DiagnosticAndExecution = Diagnostic | Execution,
        /// <summary>
        /// Traces both diangosis and garbage collector.
        /// </summary>
        DiagnosticAndGC        = Diagnostic | GC,
        /// <summary>
        /// Traces both execution and garbage collector.
        /// </summary>
        ExecutionAndGC         = Execution | GC,

        /// <summary>
        /// Traces the diagnosis, execution and garbage collector.
        /// </summary>
        All                    = Diagnostic | Execution | GC
    }
}
