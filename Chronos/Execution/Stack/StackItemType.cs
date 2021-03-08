namespace Chronos.Execution {
    /// <summary>
    /// The possible types that can be saved on the stack.
    /// </summary>
    public enum StackItemType {
        /// <summary>
        /// No type.
        /// </summary>
        None,
        /// <summary>
        /// A 32-bit integer.
        /// </summary>
        Int32,
        /// <summary>
        /// A 64-bit integer.
        /// </summary>
        Int64,
        /// <summary>
        /// An integer with native size.
        /// </summary>
        NativeInt,
        /// <summary>
        /// A floating point number (holding both floats and doubles).
        /// </summary>
        Double,
        /// <summary>
        /// A managed pointer.
        /// </summary>
        ByReference,
        /// <summary>
        /// A reference to an object.
        /// </summary>
        ObjectReference,
        /// <summary>
        /// A value type.
        /// </summary>
        ValueType
    }
}
