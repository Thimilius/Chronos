using System;

namespace Chronos.Tracing {
    /// <summary>
    /// A tracer that does nothing.
    /// </summary>
    public class NonTracer : ITracer {
        /// <summary>
        /// <inheritdoc cref="ITracer.Trace(object)"/>
        /// </summary>
        public void Trace(object value) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.Trace(string, object[])"/>
        /// </summary>
        public void Trace(string message, params object[] args) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine()"/>
        /// </summary>
        public void TraceLine() { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine(object)"/>
        /// </summary>
        public void TraceLine(object value) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine(string, object[])"/>
        /// </summary>
        public void TraceLine(string message, params object[] args) { }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColor(ConsoleColor, object)"/>
        /// </summary>
        public void TraceColor(ConsoleColor color, object value) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColor(ConsoleColor, string, object[])"/>
        /// </summary>
        public void TraceColor(ConsoleColor color, string message, params object[] args) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColorLine(ConsoleColor, object)"/>
        /// </summary>
        public void TraceColorLine(ConsoleColor color, object value) { }
        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColorLine(ConsoleColor, string, object[])"/>
        /// </summary>
        public void TraceColorLine(ConsoleColor color, string message, params object[] args) { }

        /// <summary>
        /// <inheritdoc cref="ITracer.Indent"/>
        /// </summary>
        public void Indent() { }
        /// <summary>
        /// <inheritdoc cref="ITracer.Unindent"/>
        /// </summary>
        public void Unindent() { }
    }
}
