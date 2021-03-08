using System.Diagnostics;
using System.Reflection;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a field of a type.
    /// </summary>
    public class FieldDescription : MemberDescription {
        /// <summary>
        /// The atttributes of the field.
        /// </summary>
        public FieldAttributes Attributes { get; }
        /// <summary>
        /// The type of the field.
        /// </summary>
        public TypeDescription Type { get; }
        /// <summary>
        /// The constant value of the field if present.
        /// </summary>
        public object Constant { get; }

        /// <summary>
        /// The offset of this field in memory.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Constructs a new field description.
        /// </summary>
        /// <param name="owningType">The type the field belongs to</param>
        /// <param name="name">The name of the field</param>
        /// <param name="attributes">The attributes of the field</param>
        /// <param name="type">The type of the field</param>
        /// <param name="constant">The constant value of the field if present</param>
        public FieldDescription(TypeDescription owningType, string name, FieldAttributes attributes, TypeDescription type, object constant)
            : base(owningType, name) {
            Attributes = attributes;
            Type = type;
            Constant = constant;
        }

        /// <summary>
        /// Sets the offset of this field in memory.
        /// </summary>
        /// <param name="offset">The offset of this field in memory</param>
        public void SetOffset(int offset) {
            Debug.Assert(offset >= 0);

            Offset = offset;
        }
    }
}
