using Chronos.Execution;
using System;
using System.Diagnostics;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Diagnostics.Debug class.
    /// </summary>
    public static class DebugNative {
        /// <summary>
        /// Asserts that a given condition is true.
        /// </summary>
        /// <param name="condition">The condition to assert</param>
        public static void Assert(bool condition) {
            if (!condition) {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("--- ASSERTION FAILED ---");
                foreach (IStackFrame stackFrame in VirtualMachine.ExecutionEngine.CallStack) {
                    Console.WriteLine("\tat {0}", stackFrame.Method.StackName);
                }

                // NOTE: We are not properly clearing all resources here.
#if DEBUG
                Debugger.Break();
#else
                Environment.Exit(0);
#endif
            }
        }
    }
}
