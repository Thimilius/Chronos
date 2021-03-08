using Chronos.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Chronos.Metadata {
    public static partial class MetadataSystem {
        /// <summary>
        /// Initializes the type system.
        /// </summary>
        public static void Initialize() {
            // It is important that we resolve the underlying enum types first
            // because they get used for the resolving the field layouts.
            ResolveUnderlyingEnumTypes();

            ComputeCategoryTypeFlags();
            ResolveFieldLayouts();
            ComputeDetailedTypeFlags();

            SetupMethodTables();
            SetupLocalVariables();
        }

        /// <summary>
        /// Resolves the underlying type for all enums.
        /// </summary>
        private static void ResolveUnderlyingEnumTypes() {
            var enums = s_Types.Where(t => t.BaseType == EnumType);
            foreach (TypeDescription e in enums) {
                FieldDescription field = e.InstanceFields.FirstOrDefault();
                e.SetUnderlyingType(field.Type);
            }
        }

        /// <summary>
        /// Computes the category type flags for all types.
        /// </summary>
        private static void ComputeCategoryTypeFlags() {
            foreach (TypeDescription type in s_Types) {
                type.ComputeTypeFlags(TypeFlags.CategoryMask);
            }
        }

        /// <summary>
        /// Computes the detailed type flags for all types.
        /// </summary>
        private static void ComputeDetailedTypeFlags() {
            foreach (TypeDescription type in s_Types) {
                type.ComputeTypeFlags(TypeFlags.None);
            }
        }

        /// <summary>
        /// Resolves the field layout for all type.
        /// </summary>
        private static void ResolveFieldLayouts() {
            foreach (TypeDescription type in s_Types) {
                int size = ResolveFieldLayoutForType(type);
                type.SetSize(size);
            }
        }

        /// <summary>
        /// Resolves the field layout for a type.
        /// </summary>
        /// <param name="type">The type to resolve the field layout for</param>
        /// <returns>The complete size in bytes all fields of the type (and its bases) take up</returns>
        private static int ResolveFieldLayoutForType(TypeDescription type) {
            if (type.IsSpecialSystemType) {
                switch (type.SpecialSystemType) {
                    case SpecialSystemType.Boolean: return sizeof(bool);
                    case SpecialSystemType.Char: return sizeof(char);
                    case SpecialSystemType.SByte: return sizeof(sbyte);
                    case SpecialSystemType.Byte: return sizeof(byte);
                    case SpecialSystemType.Int16: return sizeof(short);
                    case SpecialSystemType.UInt16: return sizeof(ushort);
                    case SpecialSystemType.Int32: return sizeof(int);
                    case SpecialSystemType.UInt32: return sizeof(uint);
                    case SpecialSystemType.Int64: return sizeof(long);
                    case SpecialSystemType.UInt64: return sizeof(long);
                    case SpecialSystemType.IntPtr: return IntPtr.Size;
                    case SpecialSystemType.UIntPtr: return UIntPtr.Size;
                    case SpecialSystemType.Single: return sizeof(float);
                    case SpecialSystemType.Double: return sizeof(double);
                }
            }

            // Interfaces do not have any fields which means their size is always zero.
            if (type.IsInterface) {
                return 0;
            }

            // Arrays, pointers and references are always the size of a pointer.
            if (type.IsArray || type.IsByReference || type.IsPointer) {
                return IntPtr.Size;
            }

            int size = 0;

            TypeDescription baseType = type.BaseType;
            if (baseType != null) {
                size += ResolveFieldLayoutForType(baseType);
            }

            var instanceFields = type.InstanceFields;
            foreach (FieldDescription field in instanceFields) {
                TypeDescription fieldType = field.Type;

                // We want to align the field appropriate for the type.
                int alignment = fieldType.GetAlignment();
                int padding = size % alignment;
                size += padding;

                // We save the current byte offset into the field for later use.
                field.SetOffset(size);

                if (fieldType.IsStruct) {
                    // Because of the fact that structs can not have cyclic dependencies this recursion is fine.
                    size += ResolveFieldLayoutForType(fieldType);
                } else {
                    // Normal references are always the size of a pointer.
                    size += IntPtr.Size;
                }
            }

            // The size of types should always be aligned to 8 bytes.
            size += size % 8;

            return size;
        }

        /// <summary>
        /// Sets up the method tables for all types.
        /// </summary>
        private static void SetupMethodTables() {
            foreach (TypeDescription type in s_Types) {
                // Interfaces do not need method tables.
                if (type.IsInterface) {
                    continue;
                }

                Dictionary<MethodDescription, MethodDescription> methodTable = new Dictionary<MethodDescription, MethodDescription>();

                // All our own non-abstract methods get an entry in the method table.
                IEnumerable<MethodDescription> methods = GetVirtualMethodsFromType(type);
                foreach (MethodDescription method in methods) {
                    if (!method.Attributes.HasFlag(MethodAttributes.Abstract)) {
                        methodTable[method] = method;
                    }
                }

                {
                    // All methods of interfaces get an entry if implemented and non-abstract.
                    IEnumerable<TypeDescription> interfaces = type.Interfaces;
                    foreach (TypeDescription @interface in interfaces) {
                        foreach (MethodDescription interfaceMethod in @interface.Methods) {
                            MethodDescription implementation = type.GetMethod(interfaceMethod.Name, interfaceMethod.Signature);
                            if (implementation != null && !implementation.Attributes.HasFlag(MethodAttributes.Abstract)) {
                                methodTable[interfaceMethod] = implementation;
                            }
                        }
                    }
                }

                TypeDescription baseTypeToInspect = type.BaseType;
                while (baseTypeToInspect != null) {
                    IEnumerable<MethodDescription> baseTypeMethods = GetVirtualMethodsFromType(baseTypeToInspect);

                    foreach (MethodDescription baseMethod in baseTypeMethods) {
                        // Every abstract method in the base type that is implemented by is gets an entry.
                        if (baseMethod.Attributes.HasFlag(MethodAttributes.Abstract)) {
                            MethodDescription implementation = type.GetMethod(baseMethod.Name, baseMethod.Signature);
                            if (implementation != null && !implementation.Attributes.HasFlag(MethodAttributes.Abstract)) {
                                methodTable[baseMethod] = implementation;
                            }
                        }

                        // All non-virtual methods in the base type get an entry.
                        if (!baseMethod.Attributes.HasFlag(MethodAttributes.Virtual)) {
                            methodTable[baseMethod] = baseMethod;
                        } else {
                            MethodDescription implementation = type.GetMethod(baseMethod.Name, baseMethod.Signature);

                            // We might have an implementation but that is hidden or is actually implemented in the base type.
                            if (implementation == null || baseMethod.Attributes.HasFlag(MethodAttributes.Final)) {
                                MethodDescription implementationInBase = GetImplementationInBase(type, baseMethod.Name, baseMethod.Signature);
                                methodTable[baseMethod] = implementationInBase;
                            } else if (implementation != null && !implementation.Attributes.HasFlag(MethodAttributes.Abstract)) {
                                methodTable[baseMethod] = implementation;
                            }
                        }
                    }

                    IEnumerable<TypeDescription> interfaces = baseTypeToInspect.Interfaces;
                    foreach (TypeDescription @interface in interfaces) {
                        foreach (MethodDescription interfaceMethod in @interface.Methods) {
                            // We first try to find the implementation in ourselves.
                            MethodDescription implementation = type.GetMethod(interfaceMethod.Name, interfaceMethod.Signature);
                            if (implementation != null && !implementation.Attributes.HasFlag(MethodAttributes.Abstract)) {
                                methodTable[interfaceMethod] = implementation;
                            }

                            // It might be that our base has an implementation that is hidden.
                            MethodDescription implementationInBase = baseTypeToInspect.GetMethod(interfaceMethod.Name, interfaceMethod.Signature);
                            if (implementationInBase.Attributes.HasFlag(MethodAttributes.Final)) {
                                if (implementationInBase != null && !implementationInBase.Attributes.HasFlag(MethodAttributes.Abstract)) {
                                    methodTable[interfaceMethod] = implementationInBase;
                                }
                            }
                        }
                    }

                    // Method implementations are set last, overwriting any possible set implementations.
                    foreach (var methodImplementation in baseTypeToInspect.MethodImplementations) {
                        methodTable[methodImplementation.Key] = methodImplementation.Value;
                    }

                    baseTypeToInspect = baseTypeToInspect.BaseType;
                }

                // Method implementations are set last, overwriting any possible set implementations.
                foreach (var methodImplementation in type.MethodImplementations) {
                    methodTable[methodImplementation.Key] = methodImplementation.Value;
                }


                type.SetMethodTable(methodTable);
            }
        }

        /// <summary>
        /// Tries to get the implementation of a method in the bases of a type.
        /// </summary>
        /// <param name="type">The type to get the base implementation from</param>
        /// <param name="name">The name of the method</param>
        /// <param name="signature">The signature of the method</param>
        /// <returns>The implementation of the method from on of the bases of the type</returns>
        private static MethodDescription GetImplementationInBase(TypeDescription type, string name, MethodSignatureDescription signature) {
            // Try to find the first implemented non-abstract method.
            TypeDescription baseTypeToInspect = type.BaseType;
            while (baseTypeToInspect != null) {
                MethodDescription method = baseTypeToInspect.GetMethod(name, signature);
                if (method != null && !method.Attributes.HasFlag(MethodAttributes.Abstract)) {
                    return method;
                }

                baseTypeToInspect = baseTypeToInspect.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Gets all virtual methods for the virtual method table for a given type.
        /// </summary>
        /// <param name="type">The type to get the virtual methods from</param>
        /// <returns>All virtual methods for the type</returns>
        private static IEnumerable<MethodDescription> GetVirtualMethodsFromType(TypeDescription type) {
            // Collect all relevant methods (excluding static methods, abstract methods, properties and constructors).
            return type.Methods.Where(m => m.Name != ".ctor" && m.Name != ".cctor" &&
                                      !m.Attributes.HasFlag(MethodAttributes.Static) &&
                                      !m.Attributes.HasFlag(MethodAttributes.RTSpecialName));
        }

        /// <summary>
        /// Setups the local variables for all methods.
        /// </summary>
        private static void SetupLocalVariables() {
            foreach (TypeDescription type in s_Types) {
                // We want to set the offset for each local variable and compute the overall size in bytes needed.
                foreach (MethodDescription method in type.Methods) {
                    SetupLocalVariablesForMethod(method);
                }
            }
        }

        /// <summary>
        /// Setups the local variables for a method.
        /// </summary>
        /// <param name="method">The method to setup the local variables for</param>
        private static void SetupLocalVariablesForMethod(MethodDescription method) {
            MethodBodyDescription methodBody = method.Body;
            if (methodBody == null) {
                return;
            }

            Execution.LocalVariableInfo[] locals = new Execution.LocalVariableInfo[methodBody.LocalsCount];

            int size = 0;
            for (int i = 0; i < methodBody.LocalsCount; i++) {
                TypeDescription localType = methodBody.LocalTypes[i];
                locals[i] = new Execution.LocalVariableInfo() {
                    RuntimeType = RuntimeType.FromType(localType),
                    Offset = size
                };

                if (localType.IsLargeStruct) {
                    size += localType.Size;
                } else {
                    size += VirtualMachine.ExecutionEngine.StackSlotSize;
                }
            }

            methodBody.SetLocalsSize(size);
            methodBody.SetLocals(locals);
        }
    }
}
