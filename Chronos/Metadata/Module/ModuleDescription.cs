using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing a module definition.
    /// </summary>
    public class ModuleDescription : MetadataDescription, ITokenStorage {
        /// <summary>
        /// The name of the module.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The memory info of the module
        /// </summary>
        public ModuleMemoryInfo MemoryInfo { get; }

        /// <summary>
        /// Holds all types definied in this module.
        /// </summary>
        private readonly TypeDescription[] m_TypeDefinitions;
        /// <summary>
        /// Holds all type specifications in this module.
        /// </summary>
        private readonly TypeDescription[] m_TypeSpecifications;
        /// <summary>
        /// Holds all type definitions in this module.
        /// </summary>
        private readonly TypeDescription[] m_TypeReferences;
        /// <summary>
        /// Holds all method definitions in this module.
        /// </summary>
        private readonly MethodDescription[] m_MethodDefinitions;
        /// <summary>
        /// Holds all method specifications in this module.
        /// </summary>
        private readonly MethodDescription[] m_MethodSpecifications;
        /// <summary>
        /// Holds all method references in this module.
        /// </summary>
        private readonly MethodDescription[] m_MethodReferences;
        /// <summary>
        /// Holds all field definitions in this module.
        /// </summary>
        private readonly FieldDescription[] m_FieldDefinitions;
        /// <summary>
        /// Holds all field references in this module.
        /// </summary>
        private readonly FieldDescription[] m_FieldReferences;
        /// <summary>
        /// Holds all constant strings in this module
        /// </summary>
        private readonly Dictionary<UserStringHandle, IntPtr> m_Strings;

        /// <summary>
        /// Holds the entry point of the module (if present).
        /// </summary>
        private MethodDescription m_EntryPoint;

        /// <summary>
        /// Constructs a new module description.
        /// </summary>
        /// <param name="name">The name of the module</param>
        /// <param name="memoryInfo">The memory info of the module</param>
        /// <param name="tokenCountInfo">The token count info of the module</param>
        public ModuleDescription(string name, ModuleMemoryInfo memoryInfo, ModuleTokenCountInfo tokenCountInfo) {
            Name = name;
            MemoryInfo = memoryInfo;

            // NOTE: Right now the method and field reference arrays are of the same size
            // because they are both represented by a member reference.
            // This means there is probably quite a bit of unused space in those arrays.
            // We can however not shrink it to their actual size as this would mess up the indices
            // which point into the array.
            m_TypeDefinitions = new TypeDescription[tokenCountInfo.TypeDefinitionsCount];
            m_TypeSpecifications = new TypeDescription[tokenCountInfo.TypeSpecificationsCount];
            m_TypeReferences = new TypeDescription[tokenCountInfo.TypeReferencesCount];
            m_MethodDefinitions = new MethodDescription[tokenCountInfo.MethodDefinitionsCount];
            m_MethodSpecifications = new MethodDescription[tokenCountInfo.MethodSpecificationsCount];
            m_MethodReferences = new MethodDescription[tokenCountInfo.MemberReferencesCount];
            m_FieldDefinitions = new FieldDescription[tokenCountInfo.FieldDefinitionsCount];
            m_FieldReferences = new FieldDescription[tokenCountInfo.MemberReferencesCount];

            m_Strings = new Dictionary<UserStringHandle, IntPtr>();
        }

        /// <summary>
        /// <inheritdoc cref="ITokenResolver.ResolveType(EntityHandle)"/>
        /// </summary>
        public TypeDescription ResolveType(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.TypeDefinition || token.Kind == HandleKind.TypeReference || token.Kind == HandleKind.TypeSpecification);
            int row = MetadataTokens.GetRowNumber(token) - 1;

            if (token.Kind == HandleKind.TypeDefinition) {
                return m_TypeDefinitions[row];
            } else if (token.Kind == HandleKind.TypeReference) { 
                return m_TypeReferences[row];
            } else {
                return m_TypeSpecifications[row];
            }
        }
        
        /// <summary>
        /// <inheritdoc cref="ITokenResolver.ResolveMethod(EntityHandle)"/>
        /// </summary>
        public MethodDescription ResolveMethod(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.MethodDefinition || token.Kind == HandleKind.MemberReference);
            int row = MetadataTokens.GetRowNumber(token) - 1;

            if (token.Kind == HandleKind.MethodDefinition) {
                return m_MethodDefinitions[row];
            } else {
                return m_MethodReferences[row];
            }
        }

        /// <summary>
        /// <inheritdoc cref="ITokenResolver.ResolveField(EntityHandle)"/>
        /// </summary>
        public FieldDescription ResolveField(EntityHandle token) {
            Debug.Assert(token.Kind == HandleKind.FieldDefinition || token.Kind == HandleKind.MemberReference);
            int row = MetadataTokens.GetRowNumber(token) - 1;

            if (token.Kind == HandleKind.FieldDefinition) {
                return m_FieldDefinitions[row];
            } else {
                return m_FieldReferences[row];
            }
        }

        /// <summary>
        /// <inheritdoc cref="ITokenResolver.ResolveString(UserStringHandle)"/>
        /// </summary>
        public IntPtr ResolveString(UserStringHandle token) {
            Debug.Assert(m_Strings.ContainsKey(token));

            return m_Strings[token];
        }

        /// <summary>
        /// <inheritdoc cref="ITokenStorage.StoreToken{T}(EntityHandle, T)"/>
        /// </summary>
        public void StoreToken<T>(EntityHandle token, T description) where T : MetadataDescription {
            Debug.Assert(description != null);
            int row = MetadataTokens.GetRowNumber(token) - 1;

            switch (token.Kind) {
                case HandleKind.TypeDefinition:
                    Debug.Assert(description is TypeDescription);
                    m_TypeDefinitions[row] = description as TypeDescription;
                    break;
                case HandleKind.TypeReference:
                    Debug.Assert(description is TypeDescription);
                    m_TypeReferences[row] = description as TypeDescription;
                    break;
                case HandleKind.TypeSpecification:
                    Debug.Assert(description is TypeDescription);
                    m_TypeSpecifications[row] = description as TypeDescription;
                    break;
                case HandleKind.MethodDefinition:
                    Debug.Assert(description is MethodDescription);
                    m_MethodDefinitions[row] = description as MethodDescription;
                    break;
                case HandleKind.FieldDefinition:
                    Debug.Assert(description is FieldDescription);
                    m_FieldDefinitions[row] = description as FieldDescription;
                    break;
                case HandleKind.MemberReference:
                    if (description is MethodDescription method) {
                        m_MethodReferences[row] = method;
                    } else if (description is FieldDescription field)  {
                        m_FieldReferences[row] = field;
                    } else {
                        Debug.Assert(false);
                    }
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        /// <summary>
        /// <inheritdoc cref="ITokenStorage.StoreString(UserStringHandle, IntPtr)"/>
        /// </summary>
        public void StoreString(UserStringHandle token, IntPtr pointer) {
            Debug.Assert(!m_Strings.ContainsKey(token));

            m_Strings[token] = pointer;
        }

        /// <summary>
        /// Gets a type with the given namespace and name.
        /// </summary>
        /// <param name="namespace">The namespace of the type</param>
        /// <param name="name">The name of the type</param>
        /// <returns>The type corresponding to the namespace and name</returns>
        public TypeDescription GetType(string @namespace, string name) {
            return m_TypeDefinitions.FirstOrDefault(t => t.Namespace == @namespace && t.Name == name);
        }

        /// <summary>
        /// Sets the entry point for the module.
        /// </summary>
        /// <param name="handle"></param>
        public void SetEntryPoint(EntityHandle handle) {
            m_EntryPoint = ResolveMethod(handle);
        }

        /// <summary>
        /// Gets the entry point of the module.
        /// </summary>
        /// <returns>The entry point of the module</returns>
        public MethodDescription GetEntryPoint() {
            Debug.Assert(m_EntryPoint != null);

            return m_EntryPoint;
        }
    }
}
