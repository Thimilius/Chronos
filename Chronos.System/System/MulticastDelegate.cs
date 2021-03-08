namespace System {
    public abstract class MulticastDelegate : Delegate {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private int m_InvocationCount;
        private MulticastDelegate[] m_InvocationList;
#pragma warning restore CS0169
#pragma warning restore CS0649

        public override Delegate[] GetInvocationList() {
            return m_InvocationList;
        }
    }
}
