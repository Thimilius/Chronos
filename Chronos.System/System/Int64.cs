namespace System {
    public struct Int64 {
        public const long MaxValue = 9223372036854775807;
        public const long MinValue = -9223372036854775808;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly uint m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.Int64ToString(this);
        }
    }
}
