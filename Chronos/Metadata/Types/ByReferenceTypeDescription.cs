using System.Collections.Generic;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a by reference type definition.
    /// </summary>
    public class ByReferenceTypeDescription : ParameterizedTypeDescription {
        /// <summary>
        /// Constructs a new array type description.
        /// </summary>
        /// <param name="index">The index of the type</param>
        /// <param name="elementType">The parameter type of the by reference type</param>
        public ByReferenceTypeDescription(TypeDescription parameterType) : base(parameterType) {
            // By reference types do not have any methods, fields or implement interfaces themselves.
            SetMethods(new List<MethodDescription>());
            SetFields(new List<FieldDescription>());
            SetMethodImplementations(new Dictionary<MethodDescription, MethodDescription>());
            SetInterfaces(new List<TypeDescription>());

            Name = ParameterType.ToString() + "&";
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.ComputeTypeFlagsImplementation(TypeFlags)"/>
        /// </summary>
        protected override TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                return TypeFlags.ByReference;
            } else {
                return TypeFlags.None;
            }
        }
    }
}
