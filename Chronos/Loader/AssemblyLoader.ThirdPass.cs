using Chronos.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Chronos.Loader {
    public partial class AssemblyLoader {
        private ModuleDescription DoAssemblyLoadThirdPassWithReferences(string executableDirectory, PEReader peReader, MetadataReader metadataReader) {
            Debug.Assert(executableDirectory != null);
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);

            AssemblyName assemblyName = metadataReader.GetAssemblyDefinition().GetAssemblyName();
            ModuleDescription module = m_Storage.RetrieveAssembly(assemblyName);
            DoAssemblyLoadThirdPass(metadataReader, module);

            bool hasReferenceToSystemLibrary = false;
            foreach (var assemblyReferenceHandle in metadataReader.AssemblyReferences) {
                AssemblyReference assemblyReference = metadataReader.GetAssemblyReference(assemblyReferenceHandle);
                AssemblyName assemblyReferenceName = assemblyReference.GetAssemblyName();

                // Because we already loaded 'Chronos.System' we do NOT need to load it now.
                // We want to make sure however that this executable has a reference to it.
                if (assemblyReferenceName.Name == SYSTEM_LIBRARY_NAME) {
                    hasReferenceToSystemLibrary = true;
                } else {
                    DoAssemblyLoadPassWithReferences(executableDirectory, assemblyReferenceName, DoAssemblyLoadThirdPassWithReferences);
                }
            }

            if (!hasReferenceToSystemLibrary) {
                throw new NotSupportedException("Assembly to execute does not have a reference to core system library 'Chronos.System'");
            }

            return module;
        }

        private void DoAssemblyLoadThirdPass(MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            ResolveMemberReferences(metadataReader, tokenStorage);

            LoadTypesEvenMoreDetailed(metadataReader, tokenStorage);
        }

        private void ResolveMemberReferences(MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            SignatureDecoder signatureDecoder = new SignatureDecoder(tokenStorage);

            foreach (var memberReferenceHandle in metadataReader.MemberReferences) {
                var memberReference = metadataReader.GetMemberReference(memberReferenceHandle);
                string memberName = metadataReader.GetString(memberReference.Name);
                EntityHandle parentHandle = memberReference.Parent;
                MemberReferenceKind memberReferenceKind = memberReference.GetKind();

                if (parentHandle.Kind == HandleKind.TypeReference || parentHandle.Kind == HandleKind.TypeSpecification) {
                    TypeDescription parent;
                    if (parentHandle.Kind == HandleKind.TypeReference) {
                        parent = tokenStorage.ResolveType((TypeReferenceHandle)parentHandle);
                    } else {
                        parent = tokenStorage.ResolveType((TypeSpecificationHandle)parentHandle);
                    }

                    if (memberReferenceKind == MemberReferenceKind.Method) {
                        MethodSignature<TypeDescription> decodedSignature = memberReference.DecodeMethodSignature(signatureDecoder, null);
                        MethodSignatureDescription methodSignature = new MethodSignatureDescription(decodedSignature.Header, decodedSignature.GenericParameterCount, decodedSignature.RequiredParameterCount, decodedSignature.ReturnType, decodedSignature.ParameterTypes);

                        MethodDescription method = parent.GetMethod(memberName, methodSignature);

                        tokenStorage.StoreToken(memberReferenceHandle, method);
                    } else {
                        FieldDescription field = parent.GetField(memberName);

                        tokenStorage.StoreToken(memberReferenceHandle, field);
                    }
                } else {
                    // NOTE: If we land here this could only mean a global function which we do not support right now.
                    throw new NotImplementedException();
                }
            }
        }

        private void LoadTypesEvenMoreDetailed(MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions) {
                LoadTypeEvenMoreDetailed(typeDefinitionHandle, metadataReader, tokenStorage);
            }
        }

        private void LoadTypeEvenMoreDetailed(TypeDefinitionHandle token, MetadataReader metadataReader, ITokenStorage tokenStorage) {
            var typeDefinition = metadataReader.GetTypeDefinition(token);
            TypeDescription type = tokenStorage.ResolveType(token);

            // Load method implementations
            Dictionary<MethodDescription, MethodDescription> methodImplementaions = new Dictionary<MethodDescription, MethodDescription>();
            foreach (var methodImplementationHandle in typeDefinition.GetMethodImplementations()) {
                var methodImplementation = metadataReader.GetMethodImplementation(methodImplementationHandle);
                MethodDescription implementedMethod = tokenStorage.ResolveMethod(methodImplementation.MethodDeclaration);
                MethodDescription implementationMethod = tokenStorage.ResolveMethod(methodImplementation.MethodBody);

                methodImplementaions.Add(implementedMethod, implementationMethod);
            }
            type.SetMethodImplementations(methodImplementaions);
        }
    }
}
