using System.Runtime.CompilerServices;

namespace System {
    internal class RuntimeType : Type {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly string m_FullName;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public override string FullName => m_FullName;
    }
}
