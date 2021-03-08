using System.Runtime.CompilerServices;

namespace System {
#pragma warning disable CS0659
#pragma warning disable CS0661
    public sealed class String : ICloneable {
        public static readonly string Empty;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly int m_Length;
        public int Length => m_Length;

        private readonly char m_FirstCharacter;
#pragma warning restore CS0649
#pragma warning restore CS0169

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern String(char c, int count);

        public object Clone() {
            return this;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (!(obj is string str)) {
                return false;
            }
            
            if (Length != str.Length) {
                return false;
            }

            return EqualsHelper(this, str);
        }

        public override string ToString() {
            return this;
        }

        public static bool operator ==(string a, string b) {
            return Equals(a, b);
        }

        public static bool operator !=(string a, string b) {
            return !Equals(a, b);
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern string Concat(string a, string b);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private static extern bool EqualsHelper(string a, string b);
    }
#pragma warning restore CS0659
#pragma warning restore CS0661
}