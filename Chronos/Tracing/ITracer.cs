using System;

namespace Chronos.Tracing {
    /// <summary>
    /// Interface for tracing.
    /// </summary>
    public interface ITracer {
        /// <summary>
        /// Traces an object.
        /// </summary>
        /// <param name="value">The object to trace</param>
        void Trace(object value);
        /// <summary>
        /// Traces a formatted message with given arguments.
        /// </summary>
        /// <param name="message">The formatted message to trace</param>
        /// <param name="args">The arguments corresponding to the formatted message</param>
        void Trace(string message, params object[] args);
        /// <summary>
        /// Traces a new line.
        /// </summary>
        void TraceLine();
        /// <summary>
        /// Traces an object and a new line.
        /// </summary>
        /// <param name="value">The object to trace</param>
        void TraceLine(object value);
        /// <summary>
        /// Traces a formatted message with given arguments and a new line.
        /// </summary>
        /// <param name="message">The formatted message to trace</param>
        /// <param name="args">The arguments corresponding to the formatted message</param>
        void TraceLine(string message, params object[] args);

        /// <summary>
        /// Traces an object with a given color.
        /// </summary>
        /// <param name="color">The tracing color</param>
        /// <param name="value">The object to trace</param>
        void TraceColor(ConsoleColor color, object value);
        /// <summary>
        /// Traces a formatted message with given arguments and color.
        /// </summary>
        /// <param name="color">The tracing color</param>
        /// <param name="message">The formatted message to trace</param>
        /// <param name="args">The arguments corresponding to the formatted message</param>
        void TraceColor(ConsoleColor color, string message, params object[] args);
        /// <summary>
        /// Traces an object and a new line with a given color.
        /// </summary>
        /// <param name="color">The tracing color</param>
        /// <param name="value">The object to trace</param>
        void TraceColorLine(ConsoleColor color, object value);
        /// <summary>
        /// Traces a formatted message with given arguments, color and a new line.
        /// </summary>
        /// <param name="color">The tracing color</param>
        /// <param name="message">The formatted message to trace</param>
        /// <param name="args">The arguments corresponding to the formatted message</param>
        void TraceColorLine(ConsoleColor color, string message, params object[] args);

        /// <summary>
        /// Indents a level.
        /// </summary>
        void Indent();
        /// <summary>
        /// Unindents a level.
        /// </summary>
        void Unindent();
    }
}
