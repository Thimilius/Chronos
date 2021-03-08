namespace System {
    public struct Boolean {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private bool m_Value;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string ToString() {
            return m_Value ? "True" : "False";
        }
    }
}
