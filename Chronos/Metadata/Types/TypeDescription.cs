using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a type definition.
    /// </summary>
    public class TypeDescription : MetadataDescription {
        /// <summary>
        /// The size of a small struct in bytes.
        /// </summary>
        private const int SMALL_STRUCT_SIZE = 8;
        /// <summary>
        /// The name of the static constructor method.
        /// </summary>
        private const string STATIC_CONSTRUCTOR_NAME = ".cctor";
        /// <summary>
        /// The name of the finalizer method.
        /// </summary>
        private const string FINALIZER_NAME = "Finalize";

        /// <summary>
        /// A handle to this type.
        /// </summary>
        public GCHandle Handle { get; }

        /// <summary>
        /// The module the type belongs to.
        /// </summary>
        public ModuleDescription OwningModule { get; }
        /// <summary>
        /// The namespace the type is contained in (can be empty).
        /// </summary>
        public string Namespace { get; protected set; }

        /// <summary>
        /// The name of the type.
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The full name of the type.
        /// </summary>
        public string FullName {
            get {
                if (IsNested) {
                    return  $"{Parent.FullName}.{Name}";
                } else {
                    return Namespace == string.Empty ? Name : $"{Namespace}.{Name}";
                }
            }
        }
        /// <summary>
        /// The attributes ot the type.
        /// </summary>
        public TypeAttributes Attributes { get; }
        /// <summary>
        /// The layout of the type.
        /// </summary>
        public TypeLayout Layout { get; }
        /// <summary>
        /// Indicator for whether or not this type is nested in another type.
        /// </summary>
        public bool IsNested { get; }
        /// <summary>
        /// The parent of this type if it is nested.
        /// </summary>
        public TypeDescription Parent { get; }

        /// <summary>
        /// Indicator for whether or not this type is a special system type.
        /// </summary>
        public bool IsSpecialSystemType => SpecialSystemType != SpecialSystemType.None;
        /// <summary>
        /// The special system type of the type.
        /// </summary>
        public SpecialSystemType SpecialSystemType { get; private set; }

        /// <summary>
        /// The flags of the type.
        /// </summary>
        public TypeFlags Flags { get; private set; }

        /// <summary>
        /// Indicator for whether or not this type is a primitive.
        /// </summary>
        public bool IsPrimitive => GetCategoryFlags() < TypeFlags.Class;
        /// <summary>
        /// Indicator for whether or not this type is a reference type.
        /// </summary>
        public bool IsReference => Flags.HasFlag(TypeFlags.Reference);
        /// <summary>
        /// Indicator for whether or not this type is a class.
        /// </summary>
        public bool IsClass => GetCategoryFlags() == TypeFlags.Class;
        /// <summary>
        /// Indicator for whether or not this type is System.Void.
        /// </summary>
        public bool IsVoid => GetCategoryFlags() == TypeFlags.Void;
        /// <summary>
        /// Indicator for whether or not this type is a pointer type.
        /// </summary>
        public bool IsPointer => GetCategoryFlags() == TypeFlags.Pointer;
        /// <summary>
        /// Indicator for whether or not this type is a by reference type.
        /// </summary>
        public bool IsByReference => GetCategoryFlags() == TypeFlags.ByReference;
        /// <summary>
        /// Indicator for whether or not this type is an array types.
        /// </summary>
        public bool IsArray => GetCategoryFlags() == TypeFlags.Array;
        /// <summary>
        /// Indicator for whether or not this type is a struct (excluding primitive types).
        /// </summary>
        public bool IsStruct => GetCategoryFlags() == TypeFlags.Struct;
        /// <summary>
        /// Indicator for whether or not this type is an enum.
        /// </summary>
        public bool IsEnum => GetCategoryFlags() == TypeFlags.Enum;
        /// <summary>
        /// Indicator for whether or not this type is an interface.
        /// </summary>
        public bool IsInterface => GetCategoryFlags() == TypeFlags.Interface;
        /// <summary>
        /// Indicator for whether or not this type is a delegate.
        /// </summary>
        public bool IsDelegate => GetCategoryFlags() == TypeFlags.Delegate;
        /// <summary>
        /// Indicator for whether or not this type is a large struct.
        /// </summary>
        public bool IsLargeStruct => Flags.HasFlag(TypeFlags.LargeStruct);
        /// <summary>
        /// Indicator for whether or not this type is a multidimensional array.
        /// </summary>
        public bool IsMDArray => Flags.HasFlag(TypeFlags.MDArray);

        /// <summary>
        /// Gets the base type of the type.
        /// </summary>
        public TypeDescription BaseType { get; private set; }

        /// <summary>
        /// Gets the size of the type (including fields).
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Gets the offset the static storage of this type is stored in memory.
        /// A negative number indicates that this type does not have any static memory.
        /// </summary>
        public int StaticStorageOffset { get; private set; }

        /// <summary>
        /// The instantiation of the type.
        /// </summary>
        public virtual Instantiation Instantiation => Instantiation.Empty;
        /// <summary>
        /// Indicator for whether or not this type has an instantiation.
        /// </summary>
        public bool HasInstantiation => Instantiation.Length > 0;

        /// <summary>
        /// Gets all nested types of this type.
        /// </summary>
        public IReadOnlyList<TypeDescription> NestedTypes { get; private set; }
        /// <summary>
        /// Gets all implemented interfaces of this type.
        /// </summary>
        public IReadOnlyList<TypeDescription> Interfaces { get; private set; }
        /// <summary>
        /// Gets all methods of this type.
        /// </summary>
        public IReadOnlyList<MethodDescription> Methods { get; private set; }
        /// <summary>
        /// Gets all method implementations of this type.
        /// </summary>
        public IReadOnlyDictionary<MethodDescription, MethodDescription> MethodImplementations { get; private set; }
        /// <summary>
        /// Gets all fields of this type (instance and static).
        /// </summary>
        public IReadOnlyList<FieldDescription> Fields { get; private set; }
        /// <summary>
        /// Gets all instance fields of this type.
        /// </summary>
        // NOTE: For performance we should probably cache this.
        public IEnumerable<FieldDescription> InstanceFields => Fields.Where(f => !f.Attributes.HasFlag(FieldAttributes.Static));
        /// <summary>
        /// Gets all static fields of this type.
        /// </summary>
        // NOTE: For performance we should probably cache this.
        public IEnumerable<FieldDescription> StaticFields => Fields.Where(f => f.Attributes.HasFlag(FieldAttributes.Static) && !f.Attributes.HasFlag(FieldAttributes.Literal));

        /// <summary>
        /// Gets the method table of this type.
        /// </summary>
        public IReadOnlyDictionary<MethodDescription, MethodDescription> MethodTable { get; private set; }

        /// <summary>
        /// The underlying type that is present if this type is an enum.
        /// </summary>
        private TypeDescription m_UnderlyingType;
        /// <summary>
        /// The static constructor of the type if present.
        /// </summary>
        private MethodDescription m_StaticConstructor;

        /// <summary>
        /// Constructs a new basic type description.
        /// </summary>
        /// <param name="module">The module the type belongs to</param>
        protected TypeDescription(ModuleDescription module) {
            Handle = GCHandle.Alloc(this);

            OwningModule = module;
        }

        /// <summary>
        /// Constructs a new type description with given parameters.
        /// </summary>
        /// <param name="module">The module the type belongs to</param>
        /// <param name="namespace">The namespace of the type</param>
        /// <param name="name">The name of the type</param>
        /// <param name="attributes">The attributes of the type</param>
        /// <param name="layout">The layout of the type</param>
        /// <param name="parent">The parent of a nested type</param>
        public TypeDescription(ModuleDescription module, string @namespace, string name, TypeAttributes attributes, TypeLayout layout, TypeDescription parent)
            : this (module) {
            Namespace = @namespace;
            Name = name;
            Attributes = attributes;
            Layout = layout;
            IsNested = parent != null;
            Parent = parent;
        }

        /// <summary>
        /// Checks wether this type is an instance of a given base type.
        /// This does not check for implemented interfaces.
        /// </summary>
        /// <param name="type">The base type to check</param>
        /// <returns>True if this type is an instance of the given base type</returns>
        public bool IsInstanceOfBase(TypeDescription type) {
            if (this == type) {
                return true;
            }

            TypeDescription baseType = BaseType;
            while (baseType != null) {
                if (baseType == type) {
                    return true;
                }
                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Gets a method with a given name and signature.
        /// </summary>
        /// <param name="name">The name of the method</param>
        /// <param name="signature">The signature of the method</param>
        /// <returns>The method corresponding to the name and signature</returns>
        public MethodDescription GetMethod(string name, MethodSignatureDescription signature) {
            return Methods.FirstOrDefault(m => m.Name == name && m.Signature == signature);
        }

        /// <summary>
        /// Gets a field with a given name.
        /// </summary>
        /// <param name="name">The name of the field</param>
        /// <returns>The field corresponding to the name</returns>
        public FieldDescription GetField(string name) {
            return Fields.FirstOrDefault(f => f.Name == name);
        }

        /// <summary>
        /// Gets a nested type with a given name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>The nested type corresponding to the name</returns>
        public TypeDescription GetNestedType(string name) {
            return NestedTypes.FirstOrDefault(t => t.Name == name);
        }

        /// <summary>
        /// Instantiates the signature of the type.
        /// </summary>
        /// <param name="typeInstantiation">The type instantiation</param>
        /// <param name="methodInstantiation">The method instantiation</param>
        /// <returns>The instantiated type</returns>
        public virtual TypeDescription InstantiateSignature(Instantiation typeInstantiation, Instantiation methodInstantiation) {
            return this;
        }

        /// <summary>
        /// Gets the size a variable of this type would have.
        /// </summary>
        /// <returns>The size a variable of this type would have</returns>
        public int GetVariableSize() {
            if (IsEnum) {
                return m_UnderlyingType.GetVariableSize();
            }

            if (IsStruct) {
                return Size;
            }

            return SpecialSystemType switch
            {
                SpecialSystemType.Boolean => sizeof(bool),
                SpecialSystemType.Char => sizeof(char),
                SpecialSystemType.SByte => sizeof(sbyte),
                SpecialSystemType.Byte => sizeof(byte),
                SpecialSystemType.Int16 => sizeof(short),
                SpecialSystemType.UInt16 => sizeof(ushort),
                SpecialSystemType.Int32 => sizeof(int),
                SpecialSystemType.UInt32 => sizeof(uint),
                SpecialSystemType.Int64 => sizeof(long),
                SpecialSystemType.UInt64 => sizeof(ulong),
                SpecialSystemType.IntPtr => IntPtr.Size,
                SpecialSystemType.UIntPtr => UIntPtr.Size,
                SpecialSystemType.Single => sizeof(float),
                SpecialSystemType.Double => sizeof(double),
                _ => IntPtr.Size,
            };
        }

        /// <summary>
        /// Gets the alignment required by the type.
        /// </summary>
        /// <returns></returns>
        public int GetAlignment() {
            if (IsEnum) {
                return m_UnderlyingType.GetAlignment();
            }
            
            if (IsSpecialSystemType) {
                switch (SpecialSystemType) {
                    case SpecialSystemType.Boolean: return 1;
                    case SpecialSystemType.Char: return 2;
                    case SpecialSystemType.SByte: return 1;
                    case SpecialSystemType.Byte: return 1;
                    case SpecialSystemType.Int16: return 2;
                    case SpecialSystemType.UInt16: return 2;
                    case SpecialSystemType.Int32: return 4;
                    case SpecialSystemType.UInt32: return 4;
                    case SpecialSystemType.Int64: return 8;
                    case SpecialSystemType.UInt64: return 8;
                    case SpecialSystemType.Single: return 4;
                    case SpecialSystemType.Double: return 8;
                }
            }

            // All other types (including value types) are aligned like pointers.
            return IntPtr.Size;
        }

        /// <summary>
        /// Gets the static constructor of the type if present.
        /// </summary>
        /// <returns>The static constructor of the type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual MethodDescription GetStaticConstructor() {
            return m_StaticConstructor;
        }

        /// <summary>
        /// Gets the finalizer of the type if present.
        /// </summary>
        /// <returns>The finalizer of the type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual MethodDescription GetFinalizer() {
            // Structs do not have a finalizer.
            if (IsStruct) {
                return null;
            }

            // We might have a finalizer in the base type (excluding object).
            TypeDescription typeToInspect = this;
            while (typeToInspect != null && typeToInspect != MetadataSystem.ObjectType) {
                MethodDescription finalizer = typeToInspect.Methods.FirstOrDefault(m => m.Name == FINALIZER_NAME);
                if (finalizer != null) {
                    return finalizer;
                }

                typeToInspect = typeToInspect.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Gets the underlying type that is present if this type is an enum.
        /// </summary>
        public TypeDescription GetUnderlyingType() {
            return m_UnderlyingType;
        }

        /// <summary>
        /// Gets the type flags containing the category of this type.
        /// </summary>
        /// <returns>The type flags containing the category of this type</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeFlags GetCategoryFlags() {
            return Flags & TypeFlags.CategoryMask;
        }

        /// <summary>
        /// Computes the type flags for the type for the given mask.
        /// </summary>
        /// <param name="mask">The mask to compute the type flags for</param>
        public void ComputeTypeFlags(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                Flags = ComputeTypeFlagsImplementation(mask);
            } else {
                Flags |= ComputeTypeFlagsImplementation(mask);
            }
        }

        /// <summary>
        /// Sets the base type of this type.
        /// </summary>
        /// <param name="baseType">The base type of this type</param>
        public void SetBaseType(TypeDescription baseType) {
            BaseType = baseType;
        }

        /// <summary>
        /// Sets the special system type of this type.
        /// </summary>
        /// <param name="specialSystemType">The special system type of this type</param>
        public void SetSpecialSystemType(SpecialSystemType specialSystemType) {
            SpecialSystemType = specialSystemType;
        }

        /// <summary>
        /// Sets the underlying type of this enum type.
        /// </summary>
        /// <param name="underlyingType">The underlying type of this enum type</param>
        public void SetUnderlyingType(TypeDescription underlyingType) {
            m_UnderlyingType = underlyingType;
        }

        /// <summary>
        /// Sets the size of this type.
        /// </summary>
        /// <param name="size">The size of this type</param>
        public void SetSize(int size) {
            Size = size;
        }

        /// <summary>
        /// Sets the static storage offset of this type.
        /// </summary>
        /// <param name="offset">The static storage offset of this type</param>
        public void SetStaticStorageOffset(int offset) {
            StaticStorageOffset = offset;
        }

        /// <summary>
        /// Sets the nested types of this type.
        /// </summary>
        /// <param name="nestedTypes">The nested types of this type</param>
        public void SetNestedTypes(IReadOnlyList<TypeDescription> nestedTypes) {
            NestedTypes = nestedTypes;
        }

        /// <summary>
        /// Sets the implemented interfaces of this type.
        /// </summary>
        /// <param name="interfaces">The implemented interfaces of this type</param>
        public void SetInterfaces(IReadOnlyList<TypeDescription> interfaces) {
            Interfaces = interfaces;
        }

        /// <summary>
        /// Sets the fields of this type.
        /// </summary>
        /// <param name="fields">The fields of this type</param>
        public void SetFields(IReadOnlyList<FieldDescription> fields) {
            Fields = fields;
        }

        /// <summary>
        /// Sets the methods of this type.
        /// </summary>
        /// <param name="methods">The methods of this type</param>
        public void SetMethods(IReadOnlyList<MethodDescription> methods) {
            Methods = methods;

            foreach (var method in methods) {
                // Cache static constructor.
                if (method.Attributes.HasFlag(MethodAttributes.RTSpecialName) && method.Name == STATIC_CONSTRUCTOR_NAME) {
                    m_StaticConstructor = method;
                }
            }
        }

        /// <summary>
        /// Sets the method implementations of this type.
        /// </summary>
        /// <param name="methodImplementations">The method implementations of this type</param>
        public void SetMethodImplementations(IReadOnlyDictionary<MethodDescription, MethodDescription> methodImplementations) {
            MethodImplementations = methodImplementations;
        }

        /// <summary>
        /// Sets the method table of this type.
        /// </summary>
        /// <param name="methodTable">The method table of this type</param>
        public void SetMethodTable(IReadOnlyDictionary<MethodDescription, MethodDescription> methodTable) {
            MethodTable = methodTable;
        }

        /// <summary>
        /// Sets the flag that the static constructor for this type has run.
        /// </summary>
        public void SetStaticConstructorHasRun() {
            Flags &= ~TypeFlags.StaticConstructorNeedsToRun;
        }

        /// <summary>
        /// <inheritdoc cref="object.ToString"/>
        /// </summary>
        public override string ToString() {
            if (IsSpecialSystemType) {
                return SpecialSystemType.ToTypeString();
            }

            if (IsNested) {
                return $"{Parent}.{Name}";
            } else {
                return FullName;
            }
        }

        /// <summary>
        /// Computes the actualy type flags for a given mask.
        /// </summary>
        /// <param name="mask">The mask to compute the type flags for</param>
        /// <returns></returns>
        protected virtual TypeFlags ComputeTypeFlagsImplementation(TypeFlags mask) {
            if (mask == TypeFlags.CategoryMask) {
                switch (SpecialSystemType) {
                    case SpecialSystemType.Void: return TypeFlags.Void;
                    case SpecialSystemType.Boolean: return TypeFlags.Boolean;
                    case SpecialSystemType.Char: return TypeFlags.Char;
                    case SpecialSystemType.SByte: return TypeFlags.SByte;
                    case SpecialSystemType.Byte: return TypeFlags.Byte;
                    case SpecialSystemType.Int16: return TypeFlags.Int16;
                    case SpecialSystemType.UInt16: return TypeFlags.UInt16;
                    case SpecialSystemType.Int32: return TypeFlags.Int32;
                    case SpecialSystemType.UInt32: return TypeFlags.UInt32;
                    case SpecialSystemType.Int64: return TypeFlags.Int64;
                    case SpecialSystemType.UInt64: return TypeFlags.UInt64;
                    case SpecialSystemType.IntPtr: return TypeFlags.IntPtr;
                    case SpecialSystemType.UIntPtr: return TypeFlags.UIntPtr;
                    case SpecialSystemType.Single: return TypeFlags.Single;
                    case SpecialSystemType.Double: return TypeFlags.Double;
                }

                if (BaseType == MetadataSystem.ValueTypeType && SpecialSystemType == SpecialSystemType.None) {
                    return TypeFlags.Struct;
                } else if (BaseType == MetadataSystem.MulticastDelegateType) {
                    return TypeFlags.Delegate;
                } else if (Attributes.HasFlag(TypeAttributes.Interface)) {
                    return TypeFlags.Interface;
                } else if (m_UnderlyingType != null) {
                    return TypeFlags.Enum;
                } else {
                    return TypeFlags.Class;
                }
            } else {
                TypeFlags flags = TypeFlags.None;

                if (BaseType != MetadataSystem.ValueTypeType) {
                    flags |= TypeFlags.Reference;
                } else if (Size > SMALL_STRUCT_SIZE) {
                    flags |= TypeFlags.LargeStruct;
                }

                if (GetStaticConstructor() != null) {
                    // Set the flag that we need to run the static constructor.
                    flags |= TypeFlags.StaticConstructorNeedsToRun;
                }
                if (GetFinalizer() != null) {
                    // Set the flag that we have a finalizer.
                    flags |= TypeFlags.HasFinalizer;
                }

                return flags;
            }
        }
    }
}
