using System.Runtime.CompilerServices;

namespace System {
    public abstract class Enum {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public override extern string ToString();
    }
}
