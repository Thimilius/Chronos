namespace System {
    public struct UInt64 {
        public const ulong MaxValue = 18446744073709551615;
        public const ulong MinValue = 0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly ulong m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.UInt64ToString(this);
        }
    }
}
