namespace System {
    public struct SByte {
        public const sbyte MaxValue = 127;
        public const sbyte MinValue = -128;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly sbyte m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.SByteToString(this);
        }
    }
}
