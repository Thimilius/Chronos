namespace System {
    public struct UInt16 {
        public const ushort MaxValue = 65535;
        public const ushort MinValue = 0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly ushort m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.UInt16ToString(this);
        }
    }
}
