using System.Collections.Immutable;
using System.Text;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing an instantiation of a generic type or method.
    /// </summary>
    public struct Instantiation {
        /// <summary>
        /// Gets the empty instantiation.
        /// </summary>
        public static readonly Instantiation Empty = new Instantiation(ImmutableArray<TypeDescription>.Empty);

        /// <summary>
        /// Gets the generic parameter at an index.
        /// </summary>
        /// <param name="index">The index to get the parameter of</param>
        /// <returns>The type corresponding to the index</returns>
        public TypeDescription this[int index] => m_GenericParameters[index];

        /// <summary>
        /// The length of the instantiation.
        /// </summary>
        public int Length => m_GenericParameters.Length;

        /// <summary>
        /// Indicator for whether or not this instantiation is null.
        /// </summary>
        public bool IsNull => m_GenericParameters.IsDefaultOrEmpty;

        /// <summary>
        /// Holds the generic parameters.
        /// </summary>
        private readonly ImmutableArray<TypeDescription> m_GenericParameters;

        /// <summary>
        /// Constructs a new instantiation.
        /// </summary>
        /// <param name="genericParameters">The generic parameters</param>
        public Instantiation(ImmutableArray<TypeDescription> genericParameters) {
            m_GenericParameters = genericParameters;
        }

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        public override string ToString() {
            if (IsNull) {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < m_GenericParameters.Length; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                builder.Append(m_GenericParameters[i]);
            }

            return builder.ToString();
        }
    }
}
