namespace Chronos.Metadata {
    /// <summary>
    /// Abstract base class for metadata describing a member of a type.
    /// </summary>
    public abstract class MemberDescription : MetadataDescription {
        /// <summary>
        /// The type the member belongs to.
        /// </summary>
        public TypeDescription OwningType { get; }
        /// <summary>
        /// The name of the member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs a new member description.
        /// </summary>
        /// <param name="owningType">The type the member belongs to</param>
        /// <param name="name">The name of the member</param>
        protected MemberDescription(TypeDescription owningType, string name) {
            OwningType = owningType;
            Name = name;
        }

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        public override string ToString() {
            return Name;
        }
    }
}
