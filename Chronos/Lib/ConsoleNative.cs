using Chronos.Model;
using System;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Console class.
    /// </summary>
    public static unsafe class ConsoleNative {
        /// <summary>
        /// Writes a string to the console.
        /// </summary>
        /// <param name="ptr">The string to write</param>
        public static void WriteLine(IntPtr ptr) {
            StringObject* str = (StringObject*)ptr;

            if (str == null) {
                Console.WriteLine();
            } else {
                char* buffer = ObjectModel.GetStringBuffer(str);

                // NOTE: This probably not very efficient but works for now I guess.
                string message = new string(buffer, 0, str->Length);

                Console.WriteLine(message);
            }
        }
    }
}
