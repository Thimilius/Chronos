using System.Runtime.CompilerServices;

namespace System.Diagnostics {
    public static class Debug {
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Assert(bool condition);
    }
}
