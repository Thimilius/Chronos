namespace System {
    public struct Int32 {
        public const int MaxValue = 2147483647;
        public const int MinValue = -2147483648;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly int m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.Int32ToString(this);
        }
    }
}
