using System;

namespace Chronos.Metadata {
    /// <summary>
    /// Represents flags for a type.
    /// </summary>
    [Flags]
    public enum TypeFlags {
        /// <summary>
        /// The mask for the type category.
        /// </summary>
        CategoryMask                = 0xFF,
                                  
        /// <summary>
        /// No type flags.
        /// </summary>
        None                        = 0x00,

        /// <summary>
        /// Void primitive.
        /// </summary>
        Void                        = 0x01,
        /// <summary>
        /// Boolean primitive.
        /// </summary>
        Boolean                     = 0x02,
        /// <summary>
        /// Char primitive.
        /// </summary>
        Char                        = 0x03,
        /// <summary>
        /// SByte primitive.
        /// </summary>
        SByte                       = 0x04,
        /// <summary>
        /// Byte primitive.
        /// </summary>
        Byte                        = 0x05,
        /// <summary>
        /// Int16 primitive.
        /// </summary>
        Int16                       = 0x06,
        /// <summary>
        /// UInt16 primitive.
        /// </summary>
        UInt16                      = 0x07,
        /// <summary>
        /// Int32 primitive.
        /// </summary>
        Int32                       = 0x08,
        /// <summary>
        /// UInt32 primitive.
        /// </summary>
        UInt32                      = 0x09,
        /// <summary>
        /// Int64 primitive.
        /// </summary>
        Int64                       = 0x0A,
        /// <summary>
        /// UInt64 primitive.
        /// </summary>
        UInt64                      = 0x0B,
        /// <summary>
        /// IntPtr primitive.
        /// </summary>
        IntPtr                      = 0x0C,
        /// <summary>
        /// UIntPtr primitive.
        /// </summary>
        UIntPtr                     = 0x0D,
        /// <summary>
        /// Single primitive.
        /// </summary>
        Single                      = 0x0E,
        /// <summary>
        /// Double primitive.
        /// </summary>
        Double                      = 0x0F,
        /// <summary>
        /// Class type.
        /// </summary>
        Class                       = 0x10,
        /// <summary>
        /// Value type.
        /// </summary>
        Struct                      = 0x11,
        /// <summary>
        /// Enum type.
        /// </summary>
        Enum                        = 0x12,
        /// <summary>
        /// Interface type.
        /// </summary>
        Interface                   = 0x13,
        /// <summary>
        /// Array type.
        /// </summary>
        Array                       = 0x14,
        /// <summary>
        /// ByReference type.
        /// </summary>
        ByReference                 = 0x15,
        /// <summary>
        /// Pointer type.
        /// </summary>
        Pointer                     = 0x16,
        /// <summary>
        /// Delegate type.
        /// </summary>
        Delegate                    = 0x17,

        /// <summary>
        /// Signature type variable.
        /// </summary>
        SignatureTypeVariable       = 0x18,
        /// <summary>
        /// Signature method variable.
        /// </summary>
        SignatureMethodVariable     = 0x19,

        /// <summary>
        /// Indicator for a reference type (classes, interfaces, arrays).
        /// </summary>
        Reference                   = 0x100,
        /// <summary>
        /// Indicator for a large struct.
        /// </summary>
        LargeStruct                 = 0x200,
        /// <summary>
        /// Indiactor for a multidimensional array.
        /// </summary>
        MDArray                     = 0x400,

        /// <summary>
        /// Indicator that means the type has a finalizer.
        /// </summary>
        HasFinalizer                = 0x1000,

        /// <summary>
        /// Indicator that means the static constructor for the type has not run yet.
        /// </summary>
        StaticConstructorNeedsToRun = 0x10000,
    }
}
