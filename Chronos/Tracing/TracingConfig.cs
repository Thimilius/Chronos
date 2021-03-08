using System;

namespace Chronos.Tracing {
    /// <summary>
    /// Configuration for tracing.
    /// </summary>
    public static class TracingConfig {
        /// <summary>
        /// The color used for tracing diagnostics.
        /// </summary>
        public static readonly ConsoleColor DIAGNOSTIC_COLOR = ConsoleColor.Magenta;
        /// <summary>
        /// The color used for tracing methpods.
        /// </summary>
        public static readonly ConsoleColor METHOD_COLOR = ConsoleColor.DarkCyan;
        /// <summary>
        /// The color used for tracing internal methods.
        /// </summary>
        public static readonly ConsoleColor INTERNAL_METHOD_COLOR = ConsoleColor.DarkBlue;
        /// <summary>
        /// The color used for tracing loading.
        /// </summary>
        public static readonly ConsoleColor LOADING_COLOR = ConsoleColor.DarkGreen;
        /// <summary>
        /// The color used for tracing the stack.
        /// </summary>
        public static readonly ConsoleColor STACK_COLOR = ConsoleColor.Cyan;
        /// <summary>
        /// The color used for tracing garbage collection.
        /// </summary>
        public static readonly ConsoleColor GARBAGE_COLLECTION_COLOR = ConsoleColor.Yellow;
    }
}
