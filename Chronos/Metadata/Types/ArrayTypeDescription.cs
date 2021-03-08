using System.Collections.Generic;
using System.Text;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing an array type definition.
    /// </summary>
    public class ArrayTypeDescription : ParameterizedTypeDescription {
        /// <summary>
        /// The rank of the array type.
        /// </summary>
        public int Rank { get; }

        /// <summary>
        /// Constructs a new array type description.
        /// </summary>
        /// <param name="index">The index of the type</param>
        /// <param name="elementType">The element type of the array type</param>
        /// <param name="rank">The rank of the array type</param>
        public ArrayTypeDescription(TypeDescription elementType, int rank) : base(elementType) {
            Rank = rank;

            // Arrays types do not have any methods, fields or implement interfaces themselves.
            // All their methods come from System.Array.
            SetMethods(new List<MethodDescription>());
            SetFields(new List<FieldDescription>());
            SetMethodImplementations(new Dictionary<MethodDescription, MethodDescription>());
            SetInterfaces(new List<TypeDescription>());

            Name = BuildName();
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.ComputeTypeFlagsImplementation(TypeFlags)"/>
        /// </summary>
        protected override TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                return TypeFlags.Array;
            } else {
                TypeFlags flags = TypeFlags.None;

                flags |= TypeFlags.Reference;
                if (Rank > 1) {
                    flags |= TypeFlags.MDArray;
                }

                return flags;
            }
        }

        /// <summary>
        /// Builds the name of the array type description.
        /// </summary>
        /// <returns>The name of the array type description</returns>
        private string BuildName() {
            StringBuilder builder = new StringBuilder();
            
            builder.Append(ParameterType.ToString());
            builder.Append("[");
            for (int i = 1; i < Rank; i++) {
                builder.Append(",");
            }
            builder.Append("]");

            return builder.ToString();
        }
    }
}
