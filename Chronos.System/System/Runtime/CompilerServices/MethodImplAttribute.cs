namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple= false, Inherited= false)]
    public class MethodImplAttribute : Attribute {
        public MethodImplOptions Value { get; }

        public MethodImplAttribute() { }

        public MethodImplAttribute(MethodImplOptions methodImplOptions) {
            Value = methodImplOptions;
        }

        public MethodImplAttribute(short value) {
            Value = (MethodImplOptions)value;
        }
    }
}
