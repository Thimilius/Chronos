namespace System {
    public struct Int16 {
        public const short MaxValue = 32767;
        public const short MinValue = -32768;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly short m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.Int16ToString(this);
        }
    }
}
