namespace System.Runtime.Versioning {
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class TargetFrameworkAttribute : Attribute {
        public string FrameworkName { get; }
        public string FrameworkDisplayName { get; set; }

        public TargetFrameworkAttribute(string frameworkName) {
            FrameworkName = frameworkName;
        }
    }
}