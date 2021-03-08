using System.Runtime.CompilerServices;

namespace System {
    public static class Console {
        public static void WriteLine(object value) {
            WriteLine(value.ToString());
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void WriteLine(string message);
    }
}
