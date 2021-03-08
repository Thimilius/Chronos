using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Linq;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing the signature of a method.
    /// </summary>
    public class MethodSignatureDescription : IEquatable<MethodSignatureDescription> {
        /// <summary>
        /// The header of the signature.
        /// </summary>
        public SignatureHeader Header { get; }
        /// <summary>
        /// The number of generic parameters.
        /// </summary>
        public int GenericParameterCount { get; }
        /// <summary>
        /// The number of required parameters.
        /// </summary>
        public int RequiredParameterCount { get; }
        /// <summary>
        /// The return type.
        /// </summary>
        public TypeDescription ReturnType { get; }
        /// <summary>
        /// The parameter types.
        /// </summary>
        public IList<TypeDescription> ParameterTypes { get; }

        /// <summary>
        /// Constructs a new method signature description.
        /// </summary>
        /// <param name="header">The header of the method signature</param>
        /// <param name="genericParameterCount">The number of generic parameters</param>
        /// <param name="requiredParameterCount">The number of required parameters</param>
        /// <param name="returnType">The return type</param>
        /// <param name="parameterTypes">The parameter types</param>
        public MethodSignatureDescription(SignatureHeader header, int genericParameterCount, int requiredParameterCount, TypeDescription returnType, IList<TypeDescription> parameterTypes) {
            Header = header;
            GenericParameterCount = genericParameterCount;
            RequiredParameterCount = requiredParameterCount;
            ReturnType = returnType;
            ParameterTypes = parameterTypes;
        }

        /// <summary>
        /// <inheritdoc cref="object.Equals(object?)"/>
        /// </summary>
        public override bool Equals(object obj) {
            return Equals(obj as MethodSignatureDescription);
        }

        /// <summary>
        /// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
        /// </summary>
        public bool Equals(MethodSignatureDescription other) {
            return other != null &&
                   Header.Equals(other.Header) &&
                   GenericParameterCount == other.GenericParameterCount &&
                   RequiredParameterCount == other.RequiredParameterCount &&
                   EqualityComparer<TypeDescription>.Default.Equals(ReturnType, other.ReturnType) &&
                   ParameterTypes.SequenceEqual(other.ParameterTypes);
        }

        /// <summary>
        /// <inheritdoc cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode() {
            return HashCode.Combine(Header, GenericParameterCount, RequiredParameterCount, ReturnType, ParameterTypes);
        }

        /// <summary>
        /// Determines whether or not two method signatures are equal.
        /// </summary>
        /// <param name="left">The first method signature to compare</param>
        /// <param name="right">The second method signature to compare</param>
        /// <returns>True if the method signatures are equal otherwise falsae</returns>
        public static bool operator ==(MethodSignatureDescription left, MethodSignatureDescription right) {
            return EqualityComparer<MethodSignatureDescription>.Default.Equals(left, right);
        }

        /// <summary>
        /// Determines whether or not two method signatures are not equal.
        /// </summary>
        /// <param name="left">The first method signature to compare</param>
        /// <param name="right">The second method signature to compare</param>
        /// <returns>True if the method signatures are not equal otherwise falsae</returns>
        public static bool operator !=(MethodSignatureDescription left, MethodSignatureDescription right) {
            return !(left == right);
        }
    }
}
