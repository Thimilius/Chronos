namespace System {
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public sealed class AttributeUsageAttribute : Attribute {
        public AttributeTargets ValidOn { get; }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }

        public AttributeUsageAttribute(AttributeTargets validOn) {
            ValidOn = validOn;
            Inherited = true;
        }
    }
}
