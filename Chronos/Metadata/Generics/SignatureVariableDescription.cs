namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a signature variable.
    /// </summary>
    public abstract class SignatureVariableDescription : TypeDescription {
        /// <summary>
        /// The kind of the signature variable.
        /// </summary>
        public abstract SignatureVariableKind Kind { get; }

        /// <summary>
        /// The index of the signature variable.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Constructs a new siganture variable.
        /// </summary>
        /// <param name="module">The module the signature variable belongs to</param>
        /// <param name="index">The index of the siganture variable</param>
        protected SignatureVariableDescription(ModuleDescription module, int index) : base(module) {
            Index = index;

            Namespace = string.Empty;
        }
    }
}
