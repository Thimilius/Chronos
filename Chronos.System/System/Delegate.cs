using System.Runtime.CompilerServices;

namespace System {
#pragma warning disable CS0659
#pragma warning disable CS0661
    public abstract class Delegate : ICloneable {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly object m_Target;
        public object Target => m_Target;

        private readonly IntPtr m_MethodHandle;

#pragma warning restore CS0649
#pragma warning restore CS0169

        public object Clone() {
            return MemberwiseClone();
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (!(obj is Delegate del)) {
                return false;
            }

            return m_Target == del.m_Target && m_MethodHandle == del.m_MethodHandle;
        }

        public virtual Delegate[] GetInvocationList() {
            return new Delegate[] { this };
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Delegate Combine(Delegate a, Delegate b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern Delegate Remove(Delegate a, Delegate b);

        public static bool operator ==(Delegate d1, Delegate d2) {
            if (d2 is null) {
                return d1 is null;
            }

            return ReferenceEquals(d2, d1) || d2.Equals(d1);
        }

        public static bool operator !=(Delegate d1, Delegate d2) {
            if (d2 is null) {
                return !(d1 is null);
            }

            return !ReferenceEquals(d2, d1) && !d2.Equals(d1);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool EqualTypesHelper(Delegate a, Delegate b);
    }
#pragma warning restore CS0659
#pragma warning restore CS0661
}
