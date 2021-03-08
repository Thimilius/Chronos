using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Chronos.Loader {
    public partial class AssemblyLoader {
        private ModuleDescription DoAssemblyLoadFirstPassWithReferences(string executableDirectory, PEReader peReader, MetadataReader metadataReader) {
            Debug.Assert(executableDirectory != null);
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);

            AssemblyName name = metadataReader.GetAssemblyDefinition().GetAssemblyName();
            ModuleDescription module = new ModuleDescription(name.Name, ResolveMemoryInfo(peReader), ResolveTokenCountInfo(metadataReader));

            DoAssemblyLoadFirstPass(metadataReader, module);
            m_Storage.StoreAssembly(name, module);

            bool hasReferenceToSystemLibrary = false;
            foreach (var assemblyReferenceHandle in metadataReader.AssemblyReferences) {
                AssemblyReference assemblyReference = metadataReader.GetAssemblyReference(assemblyReferenceHandle);
                AssemblyName assemblyReferenceName = assemblyReference.GetAssemblyName();

                // Because we already loaded 'Chronos.System' we do NOT need to load it now.
                // We want to make sure however that this executable has a reference to it.
                if (assemblyReferenceName.Name == SYSTEM_LIBRARY_NAME) {
                    hasReferenceToSystemLibrary = true;
                } else {
                    // We only want to load the assembly if we not already loaded it
                    if (!m_Storage.ContainsAssembly(assemblyReferenceName)) {
                        DoAssemblyLoadPassWithReferences(executableDirectory, assemblyReferenceName, DoAssemblyLoadFirstPassWithReferences);
                    }
                }
            }

            if (!hasReferenceToSystemLibrary) {
                throw new NotSupportedException("Assembly to execute does not have a reference to core system library 'Chronos.System'");
            }

            return module;
        }

        private void DoAssemblyLoadFirstPass(MetadataReader metadataReader, ModuleDescription module) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(module != null);

            // Make sure we have no method specifications as we do not support them.
            if (metadataReader.GetTableRowCount(TableIndex.MethodSpec) != 0) {
                throw new NotImplementedException();
            }

            LoadTypes(metadataReader, module);
        }

        private void LoadTypes(MetadataReader metadataReader, ModuleDescription module) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(module != null);

            foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions) {
                LoadType(typeDefinitionHandle, metadataReader, module, null);
            }
        }

        private TypeDescription LoadType(TypeDefinitionHandle token, MetadataReader metadataReader, ModuleDescription module, TypeDescription parent) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(module != null);

            TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(token);
            string typeName = metadataReader.GetString(typeDefinition.Name);
            string typeNamespace = metadataReader.GetString(typeDefinition.Namespace);
            TypeAttributes typeAttributes = typeDefinition.Attributes;
            TypeLayout typeLayout = typeDefinition.GetLayout();

            TypeDescription type = null;
            // We only create the type if it is not nested or we got called recursively which means we got passed a parent
            if (!typeDefinition.IsNested || parent != null) {
                type = new TypeDescription(module, typeNamespace, typeName, typeAttributes, typeLayout, parent);

                // We are going to store the type now before we load in the nested types
                // or otherwise the index does not match up anymore.
                MetadataSystem.StoreType(type);
                module.StoreToken(token, type);

                // Load nested types explicitly
                var nestedTypeDefinitons = typeDefinition.GetNestedTypes();
                if (nestedTypeDefinitons.Length > 0) {
                    List<TypeDescription> nestedTypes = new List<TypeDescription>();
                    foreach (var nestedTypeDefinitionHandle in typeDefinition.GetNestedTypes()) {
                        TypeDescription nestedType = LoadType(nestedTypeDefinitionHandle, metadataReader, module, type);
                        nestedTypes.Add(nestedType);
                    }
                    type.SetNestedTypes(nestedTypes);
                }
            }

            return type;
        }
    }
}
