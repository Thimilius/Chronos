using System;
using System.Diagnostics;
using System.Reflection.Metadata;

namespace Chronos.Metadata {
    public static partial class MetadataSystem {
        /// <summary>
        /// The namespace to find the special system types in.
        /// </summary>
        private const string SPECIAL_SYSTEM_TYPE_NAMESPACE = "System";

        /// <summary>
        /// The type metadata for System.Void.
        /// </summary>
        public static TypeDescription VoidType { get; private set; }
        /// <summary>
        /// The type metadata for System.Boolean.
        /// </summary>
        public static TypeDescription BooleanType { get; private set; }
        /// <summary>
        /// The type metadata for System.Char.
        /// </summary>
        public static TypeDescription CharType { get; private set; }
        /// <summary>
        /// The type metadata for System.SByte.
        /// </summary>
        public static TypeDescription SByteType { get; private set; }
        /// <summary>
        /// The type metadata for System.Byte.
        /// </summary>
        public static TypeDescription ByteType { get; private set; }
        /// <summary>
        /// The type metadata for System.Int16.
        /// </summary>
        public static TypeDescription Int16Type { get; private set; }
        /// <summary>
        /// The type metadata for System.UInt16.
        /// </summary>
        public static TypeDescription UInt16Type { get; private set; }
        /// <summary>
        /// The type metadata for System.Int32.
        /// </summary>
        public static TypeDescription Int32Type { get; private set; }
        /// <summary>
        /// The type metadata for System.UInt32.
        /// </summary>
        public static TypeDescription UInt32Type { get; private set; }
        /// <summary>
        /// The type metadata for System.Int64.
        /// </summary>
        public static TypeDescription Int64Type { get; private set; }
        /// <summary>
        /// The type metadata for System.UInt64.
        /// </summary>
        public static TypeDescription UInt64Type { get; private set; }
        /// <summary>
        /// The type metadata for System.IntPtr.
        /// </summary>
        public static TypeDescription IntPtrType { get; private set; }
        /// <summary>
        /// The type metadata for System.UIntPtr.
        /// </summary>
        public static TypeDescription UIntPtrType { get; private set; }
        /// <summary>
        /// The type metadata for System.Single.
        /// </summary>
        public static TypeDescription SingleType { get; private set; }
        /// <summary>
        /// The type metadata for System.Double.
        /// </summary>
        public static TypeDescription DoubleType { get; private set; }
        /// <summary>
        /// The type metadata for System.String.
        /// </summary>
        public static TypeDescription StringType { get; private set; }
        /// <summary>
        /// The type metadata for System.Object.
        /// </summary>
        public static TypeDescription ObjectType { get; private set; }
        /// <summary>
        /// The type metadata for System.ValueType.
        /// </summary>
        public static TypeDescription ValueTypeType { get; private set; }
        /// <summary>
        /// The type metadata for System.Enum.
        /// </summary>
        public static TypeDescription EnumType { get; private set; }
        /// <summary>
        /// The type metadata for System.Array.
        /// </summary>
        public static TypeDescription ArrayType { get; private set; }
        /// <summary>
        /// The type metadata for System.MulticastDelegate.
        /// </summary>
        public static TypeDescription MulticastDelegateType { get; private set; }
        /// <summary>
        /// The type metadata for System.Exception.
        /// </summary>
        public static TypeDescription ExceptionType { get; private set; }
        /// <summary>
        /// The type metadata for System.RuntimeTypeHandle.
        /// </summary>
        public static TypeDescription RuntimeTypeHandleType { get; private set; }

        /// <summary>
        /// The type metadata for System.RuntimeType.
        /// </summary>
        public static TypeDescription RuntimeTypeType { get; private set; }
        /// <summary>
        /// The type metadata for System.NullReferenceException.
        /// </summary>
        public static TypeDescription NullReferenceExceptionType { get; private set; }
        /// <summary>
        /// The type metadata for System.InvalidCastException.
        /// </summary>
        public static TypeDescription InvalidCastExceptionType { get; private set; }
        /// <summary>
        /// The type metadata for System.IndexOutOfRangeException.
        /// </summary>
        public static TypeDescription IndexOutOfRangeExceptionType { get; private set; }
        /// <summary>
        /// The type metadata for System.NotFiniteNumberException.
        /// </summary>
        public static TypeDescription NotFiniteNumberExceptionType { get; private set; }
        /// <summary>
        /// The type metadata for System.DivideByZeroException.
        /// </summary>
        public static TypeDescription DivideByZeroExceptionType { get; private set; }

        /// <summary>
        /// Resolves the special system types.
        /// </summary>
        /// <param name="metadataReader">The metadata reader</param>
        /// <param name="systemLibrary">The system library</param>
        public static void ResolveSpecialSystemTypes(MetadataReader metadataReader, ModuleDescription systemLibrary) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(systemLibrary != null);

            string[] specialSystemTypeNames = Enum.GetNames(typeof(SpecialSystemType));
            // We start at index 1 to skip the 'None' in SpecialSystemType.
            for (int i = 1; i < specialSystemTypeNames.Length; i++) {
                string specialSystemTypeName = specialSystemTypeNames[i];
                SpecialSystemType specialSystemType = (SpecialSystemType)i;

                foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions) {
                    var typeDefinition = metadataReader.GetTypeDefinition(typeDefinitionHandle);
                    string typeName = metadataReader.GetString(typeDefinition.Name);
                    string typeNamespace = metadataReader.GetString(typeDefinition.Namespace);

                    if (typeNamespace == SPECIAL_SYSTEM_TYPE_NAMESPACE && typeName == specialSystemTypeName) {
                        TypeDescription type = systemLibrary.ResolveType(typeDefinitionHandle);
                        Debug.Assert(type != null);

                        type.SetSpecialSystemType(specialSystemType);

                        // We cache the special system types globally.
                        SetSpecialSystemType(specialSystemType, type);
                    }
                }
            }

            // There are a few types that we set explicitly apart from the SpecialSystemType enum.
            {
                TypeDescription runtimeTypeType = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "RuntimeType");
                Debug.Assert(runtimeTypeType != null);
                RuntimeTypeType = runtimeTypeType;

                TypeDescription nullReferenceExceptionType = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "NullReferenceException");
                Debug.Assert(nullReferenceExceptionType != null);
                NullReferenceExceptionType = nullReferenceExceptionType;

                TypeDescription invalidCastExceptionType = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "InvalidCastException");
                Debug.Assert(invalidCastExceptionType != null);
                InvalidCastExceptionType = invalidCastExceptionType;

                TypeDescription indexOutOfRangeExceptionType = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "IndexOutOfRangeException");
                Debug.Assert(indexOutOfRangeExceptionType != null);
                IndexOutOfRangeExceptionType = indexOutOfRangeExceptionType;

                TypeDescription notFiniteNumberException = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "NotFiniteNumberException");
                Debug.Assert(notFiniteNumberException != null);
                NotFiniteNumberExceptionType = notFiniteNumberException;

                TypeDescription divideByZeroException = systemLibrary.GetType(SPECIAL_SYSTEM_TYPE_NAMESPACE, "DivideByZeroException");
                Debug.Assert(divideByZeroException != null);
                DivideByZeroExceptionType = divideByZeroException;
            }
        }

        /// <summary>
        /// Sets the reference for a special system type.
        /// </summary>
        /// <param name="type">The special system type to set</param>
        /// <param name="description">The metadata to set</param>
        private static void SetSpecialSystemType(SpecialSystemType type, TypeDescription description) {
            switch (type) {
                case SpecialSystemType.Void:
                    VoidType = description;
                    break;
                case SpecialSystemType.Boolean:
                    BooleanType = description;
                    break;
                case SpecialSystemType.Char:
                    CharType = description;
                    break;
                case SpecialSystemType.SByte:
                    SByteType = description;
                    break;
                case SpecialSystemType.Byte:
                    ByteType = description;
                    break;
                case SpecialSystemType.Int16:
                    Int16Type = description;
                    break;
                case SpecialSystemType.UInt16:
                    UInt16Type = description;
                    break;
                case SpecialSystemType.Int32:
                    Int32Type = description;
                    break;
                case SpecialSystemType.UInt32:
                    UInt32Type = description;
                    break;
                case SpecialSystemType.Int64:
                    Int64Type = description;
                    break;
                case SpecialSystemType.UInt64:
                    UInt64Type = description;
                    break;
                case SpecialSystemType.IntPtr:
                    IntPtrType = description;
                    break;
                case SpecialSystemType.UIntPtr:
                    UIntPtrType = description;
                    break;
                case SpecialSystemType.Single:
                    SingleType = description;
                    break;
                case SpecialSystemType.Double:
                    DoubleType = description;
                    break;
                case SpecialSystemType.Object:
                    ObjectType = description;
                    break;
                case SpecialSystemType.String:
                    StringType = description;
                    break;
                case SpecialSystemType.ValueType:
                    ValueTypeType = description;
                    break;
                case SpecialSystemType.Enum:
                    EnumType = description;
                    break;
                case SpecialSystemType.Array:
                    ArrayType = description;
                    break;
                case SpecialSystemType.MulticastDelegate:
                    MulticastDelegateType = description;
                    break;
                case SpecialSystemType.Exception:
                    ExceptionType = description;
                    break;
                case SpecialSystemType.RuntimeTypeHandle:
                    RuntimeTypeHandleType = description;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }
    }
}
