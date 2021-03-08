using System.Runtime.CompilerServices;

namespace System {
    public abstract class Array : ICloneable {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private readonly int m_Length;
        public int Length => m_Length;

        private readonly int m_Rank;
        public int Rank => m_Rank;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public int GetLength(int dimension) {
            if (dimension < 0 || dimension >= m_Rank) {
                throw new IndexOutOfRangeException();
            }

            if (m_Rank == 1) {
                return m_Length;
            }

            return GetLengthHelper(dimension);
        }

        public object Clone() {
            return MemberwiseClone();
        }

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern int GetLengthHelper(int dimension);
    }
}
