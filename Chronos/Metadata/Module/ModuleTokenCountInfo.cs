namespace Chronos.Metadata {
    /// <summary>
    /// Contains information about the token counts of a module.
    /// </summary>
    public struct ModuleTokenCountInfo {
        /// <summary>
        /// The number of type definitions.
        /// </summary>
        public int TypeDefinitionsCount { get; set; }
        /// <summary>
        /// The number of type specifications.
        /// </summary>
        public int TypeSpecificationsCount { get; set; }
        /// <summary>
        /// The number of type references.
        /// </summary>
        public int TypeReferencesCount { get; set; }
        /// <summary>
        /// The number of method definitions.
        /// </summary>
        public int MethodDefinitionsCount { get; set; }
        /// <summary>
        /// The number of method specifications.
        /// </summary>
        public int MethodSpecificationsCount { get; set; }
        /// <summary>
        /// The number of field definitions.
        /// </summary>
        public int FieldDefinitionsCount { get; set; }
        /// <summary>
        /// The number of member references.
        /// </summary>
        public int MemberReferencesCount { get; set; }
        /// <summary>
        /// The number of constant strings.
        /// </summary>
        public int StringCount { get; set; }
    }
}
