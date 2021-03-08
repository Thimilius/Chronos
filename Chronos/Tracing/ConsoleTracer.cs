using System;

namespace Chronos.Tracing {
    /// <summary>
    /// Traces to the console.
    /// </summary>
    public class ConsoleTracer : ITracer {
        /// <summary>
        /// The normal color used for tracing.
        /// </summary>
        private const ConsoleColor NORMAL_COLOR = ConsoleColor.White;

        /// <summary>
        /// Holds the current indent level.
        /// </summary>
        private int m_IndentLevel;
        /// <summary>
        /// Holds wether or not the current line needs indendation.
        /// </summary>
        private bool m_LineNeedsIndentation = true;

        /// <summary>
        /// <inheritdoc cref="ITracer.Trace(object)"/>
        /// </summary>
        public void Trace(object value) {
            Write(value.ToString());
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.Trace(string, object[])"/>
        /// </summary>
        public void Trace(string message, params object[] args) {
            Write(string.Format(message, args));
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine()"/>
        /// </summary>
        public void TraceLine() {
            m_LineNeedsIndentation = true;
            Console.WriteLine();
            m_LineNeedsIndentation = true;

        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine(object)"/>
        /// </summary>
        public void TraceLine(object value) {
            m_LineNeedsIndentation = true;
            Write(value.ToString());
            Console.WriteLine();
            m_LineNeedsIndentation = true;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceLine(string, object[])"/>
        /// </summary>
        public void TraceLine(string message, params object[] args) {
            m_LineNeedsIndentation = true;
            Write(string.Format(message, args));
            Console.WriteLine();
            m_LineNeedsIndentation = true;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColor(ConsoleColor, object)"/>
        /// </summary>
        public void TraceColor(ConsoleColor color, object value) {
            Console.ForegroundColor = color;
            Write(value.ToString());
            Console.ForegroundColor = NORMAL_COLOR;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColor(ConsoleColor, string, object[])"/>
        /// </summary>
        public void TraceColor(ConsoleColor color, string message, params object[] args) {
            Console.ForegroundColor = color;
            Write(string.Format(message, args));
            Console.ForegroundColor = NORMAL_COLOR;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColorLine(ConsoleColor, object)"/>
        /// </summary>
        public void TraceColorLine(ConsoleColor color, object value) {
            m_LineNeedsIndentation = true;
            Console.ForegroundColor = color;
            Write(value.ToString());
            Console.WriteLine();
            Console.ForegroundColor = NORMAL_COLOR;
            m_LineNeedsIndentation = true;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.TraceColorLine(ConsoleColor, string, object[])"/>
        /// </summary>
        public void TraceColorLine(ConsoleColor color, string message, params object[] args) {
            m_LineNeedsIndentation = true;
            Console.ForegroundColor = color;
            Write(string.Format(message, args));
            Console.WriteLine();
            Console.ForegroundColor = NORMAL_COLOR;
            m_LineNeedsIndentation = true;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.Indent"/>
        /// </summary>
        public void Indent() {
            m_IndentLevel++;
        }

        /// <summary>
        /// <inheritdoc cref="ITracer.Unindent"/>
        /// </summary>
        public void Unindent() {
            m_IndentLevel--;
            if (m_IndentLevel < 0) {
                m_IndentLevel = 0;
            }
        }

        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="message">The message to write to the console</param>
        private void Write(string message) {
            // Do the indendation first if the current line needs it.
            if (m_LineNeedsIndentation) {
                for (int i = 0; i < m_IndentLevel; i++) {
                    Console.Write('\t');
                }
                m_LineNeedsIndentation = false;
            }

            Console.Write(message);
        }
    }
}
