using Chronos.Metadata;
using Chronos.Model;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    /// <summary>
    /// Represents a type at runtime.
    /// </summary>
    public struct RuntimeType {
        /// <summary>
        /// Shortcut for the void type.
        /// </summary>
        public static RuntimeType Void => new RuntimeType(StackItemType.None, MetadataSystem.VoidType);
        /// <summary>
        /// Shortcut for the 32-bit integer type.
        /// </summary>
        public static RuntimeType Int32 => new RuntimeType(StackItemType.Int32, MetadataSystem.Int32Type);
        /// <summary>
        /// Shortcut for the 64-bit integer type.
        /// </summary>
        public static RuntimeType Int64 => new RuntimeType(StackItemType.Int64, MetadataSystem.Int64Type);
        /// <summary>
        /// Shortcut for the native size integer type.
        /// </summary>
        public static RuntimeType NativeInt => new RuntimeType(StackItemType.NativeInt, MetadataSystem.IntPtrType);
        /// <summary>
        /// Shortcut for the double type.
        /// </summary>
        public static RuntimeType Double => new RuntimeType(StackItemType.Double, MetadataSystem.DoubleType);
        /// <summary>
        /// Shortcut for an object reference type as null.
        /// </summary>
        public static RuntimeType Null => new RuntimeType(StackItemType.ObjectReference, null);
        /// <summary>
        /// Shortcut for an object reference type as string.
        /// </summary>
        public static RuntimeType String => new RuntimeType(StackItemType.ObjectReference, MetadataSystem.StringType);

        /// <summary>
        /// The stack item type.
        /// </summary>
        public StackItemType ItemType { get; }
        /// <summary>
        /// The actual type metadata description.
        /// </summary>
        public TypeDescription Type { get; }

        /// <summary>
        /// Creates a new managed pointer runtime type to a given type.
        /// </summary>
        /// <param name="type">The type to point to</param>
        /// <returns>The managed pointer runtime type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RuntimeType FromReference(TypeDescription type) {
            Debug.Assert(type != null);

            return new RuntimeType(StackItemType.ByReference, type);
        }

        /// <summary>
        /// Creates a new unmanaged pointer runtime type to a given type.
        /// </summary>
        /// <param name="type">The type to point to</param>
        /// <returns>The unmanaged pointer runtime type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RuntimeType FromPointer(TypeDescription type) {
            Debug.Assert(type != null);

            return new RuntimeType(StackItemType.NativeInt, type);
        }

        /// <summary>
        /// Creates a new object reference runtime type to a given type.
        /// </summary>
        /// <param name="type">The type of the object reference</param>
        /// <returns>The object reference runtime type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RuntimeType FromObjectReference(TypeDescription type) {
            Debug.Assert(type != null);

            return new RuntimeType(StackItemType.ObjectReference, type);
        }

        /// <summary>
        /// Creates a new runtime type from a given type.
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The resolved runtime type</returns>
        public static RuntimeType FromType(TypeDescription type) {
            Debug.Assert(type != null);

            // If we encounter an enum we only care about the underlying type.
            if (type.IsEnum) {
                type = type.GetUnderlyingType();
            }
            
            switch (type.SpecialSystemType) {
                case SpecialSystemType.Void:
                    return new RuntimeType(StackItemType.None, MetadataSystem.VoidType);
                case SpecialSystemType.Boolean:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.BooleanType);
                case SpecialSystemType.Char:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.CharType);
                case SpecialSystemType.SByte:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.SByteType);
                case SpecialSystemType.Byte:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.ByteType);
                case SpecialSystemType.Int16:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.Int16Type);
                case SpecialSystemType.UInt16:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.UInt16Type);
                case SpecialSystemType.Int32:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.Int32Type);
                case SpecialSystemType.UInt32:
                    return new RuntimeType(StackItemType.Int32, MetadataSystem.UInt32Type);
                case SpecialSystemType.Int64:
                    return new RuntimeType(StackItemType.Int64, MetadataSystem.Int64Type);
                case SpecialSystemType.UInt64:
                    return new RuntimeType(StackItemType.Int64, MetadataSystem.UInt64Type);
                case SpecialSystemType.IntPtr:
                    return new RuntimeType(StackItemType.NativeInt, MetadataSystem.IntPtrType);
                case SpecialSystemType.UIntPtr:
                    return new RuntimeType(StackItemType.NativeInt, MetadataSystem.UIntPtrType);
                case SpecialSystemType.Single:
                    return new RuntimeType(StackItemType.Double, MetadataSystem.SingleType);
                case SpecialSystemType.Double:
                    return new RuntimeType(StackItemType.Double, MetadataSystem.DoubleType);
            }

            if (type.IsPointer || type.IsByReference) {
                return new RuntimeType(StackItemType.ByReference, type);
            } else if (type.IsStruct) {
                return new RuntimeType(StackItemType.ValueType, type);
            } else {
                return new RuntimeType(StackItemType.ObjectReference, type);
            }
        }

        /// <summary>
        /// Constructs a new runtime type with given parameters.
        /// </summary>
        /// <param name="itemType">The stack item type</param>
        /// <param name="type">The metadata type description</param>
        private RuntimeType(StackItemType itemType, TypeDescription type) {
            ItemType = itemType;
            Type = type;
        }
    }
}
