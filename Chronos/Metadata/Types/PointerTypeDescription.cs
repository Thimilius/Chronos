using System.Collections.Generic;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a pointer type definition.
    /// </summary>
    public class PointerTypeDescription : ParameterizedTypeDescription {
        /// <summary>
        /// Constructs a new pointer type description.
        /// </summary>
        /// <param name="index">The index of the type</param>
        /// <param name="parameterType">The parameter type of the pointer type</param>
        public PointerTypeDescription(TypeDescription parameterType) : base(parameterType) {
            // Pointer types do not have any methods, fields or implement interfaces themselves.
            SetMethods(new List<MethodDescription>());
            SetFields(new List<FieldDescription>());
            SetMethodImplementations(new Dictionary<MethodDescription, MethodDescription>());
            SetInterfaces(new List<TypeDescription>());

            Name = ParameterType.ToString() + "*";
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.ComputeTypeFlagsImplementation(TypeFlags)"/>
        /// </summary>
        protected override TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                return TypeFlags.Pointer;
            } else {
                return TypeFlags.None;
            }
        }
    }
}
