namespace System {
    public struct Char {
        public const char MaxValue = (char)0xFFFF;
        public const char MinValue = (char)0x0;

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly char m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return Number.CharToString(this);
        }
    }
}
