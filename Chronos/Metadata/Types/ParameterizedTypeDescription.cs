namespace Chronos.Metadata {
    /// <summary>
    /// Abstract base class for metadata type definition that has a parameter.
    /// </summary>
    public abstract class ParameterizedTypeDescription : TypeDescription {
        /// <summary>
        /// The parameter type of this type.
        /// </summary>
        public TypeDescription ParameterType { get; }

        /// <summary>
        /// Constructs a new parameterized type description.
        /// </summary>
        /// <param name="parameterType">The parameter type of the type</param>
        public ParameterizedTypeDescription(TypeDescription parameterType) : base (parameterType.OwningModule) {
            ParameterType = parameterType;

            Namespace = string.Empty;
        }
    }
}
