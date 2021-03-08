using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Chronos.Lib {
    /// <summary>
    /// Native functions for the System.Enum class.
    /// </summary>
    public static unsafe class EnumNative {
        /// <summary>
        /// Converts the enum to a string.
        /// </summary>
        /// <param name="this">The pointer to the enum</param>
        /// <returns>The converted string</returns>
        public static IntPtr ToString(IntPtr @this) {
            ObjectBase* obj = (ObjectBase*)@this;

            TypeDescription type = obj->Type;
            Debug.Assert(type.IsEnum);
            TypeDescription underlyingType = type.GetUnderlyingType();

            byte* memory = ObjectModel.GetObjectData(obj);

            var literals = type.Fields.Where(f => f.Attributes.HasFlag(FieldAttributes.Literal));
            foreach (FieldDescription field in literals) {
                if (IsEqualToFieldConstant(underlyingType, memory, field.Constant)) {
                    return ObjectModel.AllocateStringFromLiteral(field.Name);
                }
            }

            // NOTE: This is not a very elegant implementation as it does not handle flags in a nice way.

            return ObjectModel.AllocateStringFromLiteral(GetConstantStringLiteral(underlyingType, memory));
        }

        /// <summary>
        /// Checks whether or not a field constant is equal to a enum value.
        /// </summary>
        /// <param name="underlyingType">The underlying type of the enum</param>
        /// <param name="memory">A pointer to the memory containing the value of the enum</param>
        /// <param name="constant">The field constant to check</param>
        /// <returns>True of the enum value is equal to the field constant otherwise false</returns>
        private static bool IsEqualToFieldConstant(TypeDescription underlyingType, byte* memory, object constant) {
            switch (underlyingType.SpecialSystemType) {
                case SpecialSystemType.SByte:
                    return (*(sbyte*)memory) == (sbyte)constant;
                case SpecialSystemType.Byte:
                    return (*memory) == (byte)constant;
                case SpecialSystemType.Int16:
                    return (*(short*)memory) == (short)constant;
                case SpecialSystemType.UInt16:
                    return (*(ushort*)memory) == (ushort)constant;
                case SpecialSystemType.Int32:
                    return (*(int*)memory) == (int)constant;
                case SpecialSystemType.UInt32:
                    return (*(uint*)memory) == (uint)constant;
                case SpecialSystemType.Int64:
                    return (*(long*)memory) == (long)constant;
                case SpecialSystemType.UInt64:
                    return (*(ulong*)memory) == (ulong)constant;
                default:
                    Debug.Assert(false);
                    return false;
            }
        }

        /// <summary>
        /// Gets the string literal for the enum value.
        /// </summary>
        /// <param name="underlyingType">The underlying type of the enum</param>
        /// <param name="memory">A pointer to the memory containing the value of the enum</param>
        /// <returns>The string literal for the enum value</returns>
        private static string GetConstantStringLiteral(TypeDescription underlyingType, byte* memory) {
            switch (underlyingType.SpecialSystemType) {
                case SpecialSystemType.SByte:
                    return (*(sbyte*)memory).ToString();
                case SpecialSystemType.Byte:
                    return (*memory).ToString();
                case SpecialSystemType.Int16:
                    return (*(short*)memory).ToString();
                case SpecialSystemType.UInt16:
                    return (*(ushort*)memory).ToString();
                case SpecialSystemType.Int32:
                    return (*(int*)memory).ToString();
                case SpecialSystemType.UInt32:
                    return (*(uint*)memory).ToString();
                case SpecialSystemType.Int64:
                    return (*(long*)memory).ToString();
                case SpecialSystemType.UInt64:
                    return (*(ulong*)memory).ToString();
                default:
                    Debug.Assert(false);
                    return string.Empty;
            }
        }
    }
}
