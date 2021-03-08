namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a signature variable of a method.
    /// </summary>
    public class SignatureMethodVariableDescription : SignatureVariableDescription {
        /// <summary>
        /// <inheritdoc cref="SignatureVariableDescription.Kind"/>
        /// </summary>
        public override SignatureVariableKind Kind => SignatureVariableKind.Method;

        /// <summary>
        /// Constructs a new signature method variable.
        /// </summary>
        /// <param name="module">The module the signature method variable belongs to</param>
        /// <param name="index">The index of the siganture method variable</param>
        public SignatureMethodVariableDescription(ModuleDescription module, int index) : base(module, index) {
            Name = "TM" + index;
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.InstantiateSignature(Instantiation, Instantiation)"/>
        /// </summary>
        public override TypeDescription InstantiateSignature(Instantiation typeInstantiation, Instantiation methodInstantiation) {
            return methodInstantiation.IsNull ? this : methodInstantiation[Index];
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.ComputeTypeFlagsImplementation(TypeFlags)"/>
        /// </summary>
        protected override TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                return TypeFlags.SignatureMethodVariable;
            } else {
                return TypeFlags.None;
            }
        }
    }
}
