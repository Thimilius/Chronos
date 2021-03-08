namespace System {
    public struct UInt32 {
        public const uint MaxValue = 4294967295;
        public const uint MinValue = 0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly uint m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.UInt32ToString(this);
        }
    }
}
