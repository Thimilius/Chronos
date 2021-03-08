using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a method of a type.
    /// </summary>
    public class MethodDescription : MemberDescription {
        /// <summary>
        /// The handle to this method.
        /// </summary>
        public GCHandle Handle { get; }

        /// <summary>
        /// The attributes of the method.
        /// </summary>
        public MethodAttributes Attributes { get; }
        /// <summary>
        /// The implementation attributes of the method.
        /// </summary>
        public MethodImplAttributes ImplAttributes { get; }
        /// <summary>
        /// The signature of the method.
        /// </summary>
        public MethodSignatureDescription Signature { get; }
        /// <summary>
        /// The body of the method.
        /// </summary>
        public MethodBodyDescription Body { get; }

        /// <summary>
        /// The full name of the method (includes the signature).
        /// </summary>
        public string FullName { get; }
        /// <summary>
        /// The stack name of the method (includes the signature without the return type).
        /// </summary>
        public string StackName { get; }

        /// <summary>
        /// Constructs a new method description.
        /// </summary>
        /// <param name="owningType">The type the method belongs to</param>
        /// <param name="name">The name of the method</param>
        /// <param name="attributes">The attributes of the method</param>
        /// <param name="implAttributes">The implementation attributes of the method</param>
        /// <param name="signature">The signature of the method</param>
        /// <param name="body">The body of the method</param>
        public MethodDescription(TypeDescription owningType, string name, MethodAttributes attributes, MethodImplAttributes implAttributes, MethodSignatureDescription signature, MethodBodyDescription body)
            : base(owningType, name) {
            Handle = GCHandle.Alloc(this);

            Attributes = attributes;
            ImplAttributes = implAttributes;
            Signature = signature;
            Body = body;

            FullName = BuildFullName();
            StackName = BuildStackName();
        }

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        public override string ToString() {
            return FullName;
        }

        /// <summary>
        /// Builds the full name for the method.
        /// </summary>
        /// <returns>The full name of the method</returns>
        private string BuildFullName() {
            StringBuilder builder = new StringBuilder();

            builder.Append(Signature.ReturnType.ToString());
            builder.Append(" ");
            builder.Append(OwningType.FullName);
            builder.Append(".");
            builder.Append(Name);
            builder.Append("(");
            for (int i = 0; i < Signature.ParameterTypes.Count; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                builder.Append(Signature.ParameterTypes[i]);
            }
            builder.Append(")");

            return builder.ToString();
        }

        /// <summary>
        /// Builds the stack name for the method.
        /// </summary>
        /// <returns>The stack name of the method</returns>
        private string BuildStackName() {
            StringBuilder builder = new StringBuilder();

            builder.Append(OwningType.FullName);
            builder.Append(".");
            builder.Append(Name);
            builder.Append("(");
            for (int i = 0; i < Signature.ParameterTypes.Count; i++) {
                if (i > 0) {
                    builder.Append(", ");
                }
                builder.Append(Signature.ParameterTypes[i]);
            }
            builder.Append(")");

            return builder.ToString();
        }
    }
}
