namespace System {
    public struct Byte {
        public const byte MaxValue = 255;
        public const byte MinValue = 0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly byte m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.ByteToString(this);
        }
    }
}
