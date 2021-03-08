using System;

namespace Chronos.Metadata {
    /// <summary>
    /// Specifies special system types.
    /// </summary>
    public enum SpecialSystemType {
        /// <summary>
        /// No special type.
        /// </summary>
        None,

        /// <summary>
        /// Void type.
        /// </summary>
        Void,

        /// <summary>
        /// Boolean type.
        /// </summary>
        Boolean,
        /// <summary>
        /// Char type.
        /// </summary>
        Char,
        /// <summary>
        /// SByte type.
        /// </summary>
        SByte,
        /// <summary>
        /// Byte type.
        /// </summary>
        Byte,
        /// <summary>
        /// Int16 type.
        /// </summary>
        Int16,
        /// <summary>
        /// UInt16 type.
        /// </summary>
        UInt16,
        /// <summary>
        /// Int32 type.
        /// </summary>
        Int32,
        /// <summary>
        /// UInt32 type.
        /// </summary>
        UInt32,
        /// <summary>
        /// Int64 type.
        /// </summary>
        Int64,
        /// <summary>
        /// UInt64 type.
        /// </summary>
        UInt64,

        /// <summary>
        /// IntPtr type.
        /// </summary>
        IntPtr,
        /// <summary>
        /// UIntPtr type.
        /// </summary>
        UIntPtr,

        /// <summary>
        /// Single type.
        /// </summary>
        Single,
        /// <summary>
        /// Double type.
        /// </summary>
        Double,

        /// <summary>
        /// Object type.
        /// </summary>
        Object,
        /// <summary>
        /// String type.
        /// </summary>
        String,
        /// <summary>
        /// ValueType type.
        /// </summary>
        ValueType,
        /// <summary>
        /// Enum type.
        /// </summary>
        Enum,
        /// <summary>
        /// Array type.
        /// </summary>
        Array,
        /// <summary>
        /// MulticastDelegate type.
        /// </summary>
        MulticastDelegate,
        /// <summary>
        /// Exception type.
        /// </summary>
        Exception,
        /// <summary>
        /// RuntimeTypeHandle type.
        /// </summary>
        RuntimeTypeHandle
    }

    /// <summary>
    /// Extensions to the special system type enum.
    /// </summary>
    public static class SpecialSystemTypeExtensions {
        /// <summary>
        /// Converts the special system type to a corresponding C# type string.
        /// </summary>
        /// <param name="specialSystemType">The special system type</param>
        /// <returns>The corresponding C# type string</returns>
        public static string ToTypeString(this SpecialSystemType specialSystemType) {
            return specialSystemType switch {
                SpecialSystemType.Void => "void",
                SpecialSystemType.Boolean => "bool",
                SpecialSystemType.Char => "char",
                SpecialSystemType.SByte => "sbyte",
                SpecialSystemType.Byte => "byte",
                SpecialSystemType.Int16 => "short",
                SpecialSystemType.UInt16 => "ushort",
                SpecialSystemType.Int32 => "int",
                SpecialSystemType.UInt32 => "uint",
                SpecialSystemType.Int64 => "long",
                SpecialSystemType.UInt64 => "ulong",
                SpecialSystemType.IntPtr => "System.IntPtr",
                SpecialSystemType.UIntPtr => "System.UIntPtr",
                SpecialSystemType.Single => "float",
                SpecialSystemType.Double => "double",
                SpecialSystemType.String => "string",
                SpecialSystemType.Object => "object",
                SpecialSystemType.ValueType => "System.ValueType",
                SpecialSystemType.Enum => "System.Enum",
                SpecialSystemType.Array => "System.Array",
                SpecialSystemType.MulticastDelegate => "System.MulticastDelegate",
                SpecialSystemType.Exception => "System.Exception",
                SpecialSystemType.RuntimeTypeHandle => "System.RuntimeTypeHandle",
                _ => throw new NotImplementedException()
            };
        }
    }
}
