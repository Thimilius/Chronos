using System.Runtime.CompilerServices;

namespace System {
    public static class GC {
        public static void KeepAlive(object obj) {

        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void Collect();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void SuppressFinalize(object obj);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern void ReRegisterForFinalize(object obj);
    }
}
