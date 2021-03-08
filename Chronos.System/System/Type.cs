using System.Runtime.CompilerServices;

namespace System {
    public abstract class Type {
        public abstract string FullName { get; }

        public override string ToString() {
            return FullName;
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Type GetTypeFromHandle(RuntimeTypeHandle handle);
    }
}
