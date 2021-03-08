namespace System {
#pragma warning disable CS0660
#pragma warning disable CS0661
    public unsafe struct UIntPtr {
        public static readonly UIntPtr Zero = new UIntPtr(null);

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly void* m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public UIntPtr(int value) {
            m_Value = (void*)value;
        }

        public UIntPtr(long value) {
            m_Value = (void*)value;
        }

        public UIntPtr(void* value) {
            m_Value = value;
        }

        public uint ToIntU32() {
            return (uint)m_Value;
        }

        public ulong ToIntU64() {
            return (ulong)m_Value;
        }

        public void* ToPointer() {
            return m_Value;
        }

        public override string ToString() {
            return Number.UIntPtrToString(this);
        }

        public static bool operator ==(UIntPtr value1, UIntPtr value2) {
            return value1.m_Value == value2.m_Value;
        }

        public static bool operator !=(UIntPtr value1, UIntPtr value2) {
            return !(value1.m_Value == value2.m_Value);
        }
    }
#pragma warning restore CS0660
#pragma warning restore CS0661
}
