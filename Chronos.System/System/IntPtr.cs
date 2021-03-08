namespace System {
#pragma warning disable CS0660
#pragma warning disable CS0661
    public unsafe struct IntPtr {
        public static readonly IntPtr Zero = new IntPtr(null);

#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly void* m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public IntPtr(int value) {
            m_Value = (void*)value;
        }

        public IntPtr(long value) {
            m_Value = (void*)value;
        }

        public IntPtr(void* value) {
            m_Value = value;
        }

        public int ToInt32() {
            return (int)m_Value;
        }

        public long ToInt64() {
            return (long)m_Value;
        }

        public void* ToPointer() {
            return m_Value;
        }

        public override string ToString() {
            return Number.IntPtrToString(this);
        }

        public static bool operator ==(IntPtr value1, IntPtr value2) {
            return value1.m_Value == value2.m_Value;
        }

        public static bool operator !=(IntPtr value1, IntPtr value2) {
            return !(value1.m_Value == value2.m_Value);
        }
    }
#pragma warning restore CS0660
#pragma warning restore CS0661
}
