namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a signature variable of a type.
    /// </summary>
    public class SignatureTypeVariableDescription : SignatureVariableDescription {
        /// <summary>
        /// <inheritdoc cref="SignatureVariableDescription.Kind"/>
        /// </summary>
        public override SignatureVariableKind Kind => SignatureVariableKind.Type;

        /// <summary>
        /// Constructs a new signature type variable.
        /// </summary>
        /// <param name="module">The module the signature type variable belongs to</param>
        /// <param name="index">The index of the siganture type variable</param>
        public SignatureTypeVariableDescription(ModuleDescription module, int index) : base(module, index) {
            Name = "TT" + index;
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.InstantiateSignature(Instantiation, Instantiation)"/>
        /// </summary>
        public override TypeDescription InstantiateSignature(Instantiation typeInstantiation, Instantiation methodInstantiation) {
            return typeInstantiation.IsNull ? this : typeInstantiation[Index];
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.ComputeTypeFlagsImplementation(TypeFlags)"/>
        /// </summary>
        protected override TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                return TypeFlags.SignatureTypeVariable;
            } else {
                return TypeFlags.None;
            }
        }
    }
}
