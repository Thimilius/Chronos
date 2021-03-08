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
        private ModuleDescription DoAssemblyLoadSecondPassWithReferences(string executableDirectory, PEReader peReader, MetadataReader metadataReader) {
            Debug.Assert(executableDirectory != null);
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);

            AssemblyName assemblyName = metadataReader.GetAssemblyDefinition().GetAssemblyName();
            ModuleDescription module = m_Storage.RetrieveAssembly(assemblyName);
            DoAssemblyLoadSecondPass(peReader, metadataReader, module);

            bool hasReferenceToSystemLibrary = false;
            foreach (var assemblyReferenceHandle in metadataReader.AssemblyReferences) {
                AssemblyReference assemblyReference = metadataReader.GetAssemblyReference(assemblyReferenceHandle);
                AssemblyName assemblyReferenceName = assemblyReference.GetAssemblyName();

                // Because we already loaded 'Chronos.System' we do NOT need to load it now.
                // We want to make sure however that this executable has a reference to it.
                if (assemblyReferenceName.Name == SYSTEM_LIBRARY_NAME) {
                    hasReferenceToSystemLibrary = true;
                } else {
                    DoAssemblyLoadPassWithReferences(executableDirectory, assemblyReferenceName, DoAssemblyLoadSecondPassWithReferences);
                }
            }

            if (!hasReferenceToSystemLibrary) {
                throw new NotSupportedException("Assembly to execute does not have a reference to core system library 'Chronos.System'");
            }

            return module;
        }

        private void DoAssemblyLoadSecondPass(PEReader peReader, MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            ResolveTypeReferences(metadataReader, tokenStorage);

            SignatureDecoder signatureDecoder = new SignatureDecoder(tokenStorage);
            LoadTypeSpecifications(metadataReader, tokenStorage, signatureDecoder);
            LoadTypesDetailed(peReader, metadataReader, tokenStorage, signatureDecoder);

            LoadStrings(metadataReader, tokenStorage);
        }

        private void ResolveTypeReferences(MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            foreach (var typeReferenceHandle in metadataReader.TypeReferences) {
                ResolveTypeReference(typeReferenceHandle, metadataReader, tokenStorage);
            }
        }

        private TypeDescription ResolveTypeReference(TypeReferenceHandle handle, MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            var typeReferenceDefinition = metadataReader.GetTypeReference(handle);
            string typeReferenceName = metadataReader.GetString(typeReferenceDefinition.Name);
            string typeReferenceNamespace = metadataReader.GetString(typeReferenceDefinition.Namespace);
            EntityHandle resolutionScopeHandle = typeReferenceDefinition.ResolutionScope;

            TypeDescription type;
            if (resolutionScopeHandle.Kind == HandleKind.AssemblyReference) {
                AssemblyReference assemblyReference = metadataReader.GetAssemblyReference((AssemblyReferenceHandle)resolutionScopeHandle);
                AssemblyName assemblyReferenceName = assemblyReference.GetAssemblyName();

                type = m_Storage.RetrieveAssembly(assemblyReferenceName).GetType(typeReferenceNamespace, typeReferenceName);
            } else if (resolutionScopeHandle.Kind == HandleKind.TypeReference) {
                // Nested type so recursively try to resolve the reference
                TypeDescription parent = ResolveTypeReference((TypeReferenceHandle)resolutionScopeHandle, metadataReader, tokenStorage);
                type = parent.GetNestedType(typeReferenceName);
            } else {
                throw new NotImplementedException();
            }

            tokenStorage.StoreToken(handle, type);

            return type;
        }

        private void LoadTypeSpecifications(MetadataReader metadataReader, ITokenStorage tokenStorage, SignatureDecoder signatureDecoder) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);
            Debug.Assert(signatureDecoder != null);

            for (int i = 1; i <= metadataReader.GetTableRowCount(TableIndex.TypeSpec); i++) {
                TypeSpecificationHandle typeSpecificationHandle = MetadataTokens.TypeSpecificationHandle(i);
                TypeSpecification typeSpecification = metadataReader.GetTypeSpecification(typeSpecificationHandle);
                TypeDescription type = typeSpecification.DecodeSignature(signatureDecoder, null);
                tokenStorage.StoreToken(typeSpecificationHandle, type);
            }
        }

        private void LoadTypesDetailed(PEReader peReader, MetadataReader metadataReader, ITokenStorage tokenStorage, SignatureDecoder signatureDecoder) {
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            foreach (var typeDefinitionHandle in metadataReader.TypeDefinitions) {
                LoadTypeDetailed(typeDefinitionHandle, peReader, metadataReader, tokenStorage, signatureDecoder);
            }
        }

        private void LoadTypeDetailed(TypeDefinitionHandle token, PEReader peReader, MetadataReader metadataReader, ITokenStorage tokenStorage, SignatureDecoder signatureDecoder) {
            Debug.Assert(peReader != null);
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);
            Debug.Assert(signatureDecoder != null);

            var typeDefinition = metadataReader.GetTypeDefinition(token);
            TypeDescription type = tokenStorage.ResolveType(token);

            // Load base type
            TypeDescription baseType = null;
            if (!typeDefinition.BaseType.IsNil) {
                baseType = tokenStorage.ResolveType(typeDefinition.BaseType);
            }
            type.SetBaseType(baseType);

            // Load interface implementations.
            List<TypeDescription> interfaces = new List<TypeDescription>();
            foreach (var interfaceImplementationHandle in typeDefinition.GetInterfaceImplementations()) {
                InterfaceImplementation interfaceImplementation = metadataReader.GetInterfaceImplementation(interfaceImplementationHandle);
                TypeDescription @interface = tokenStorage.ResolveType(interfaceImplementation.Interface);
                interfaces.Add(@interface);
            }
            type.SetInterfaces(interfaces);

            LoadFields(typeDefinition, type, metadataReader, tokenStorage, signatureDecoder);
            LoadMethods(typeDefinition, type, metadataReader, peReader, tokenStorage, signatureDecoder);
        }

        private void LoadFields(TypeDefinition typeDefinition, TypeDescription type, MetadataReader metadataReader, ITokenStorage tokenStorage, SignatureDecoder signatureDecoder) {
            Debug.Assert(type != null);
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);
            Debug.Assert(signatureDecoder != null);

            List<FieldDescription> fields = new List<FieldDescription>();
            foreach (var fieldDefinitionHandle in typeDefinition.GetFields()) {
                var fieldDefinition = metadataReader.GetFieldDefinition(fieldDefinitionHandle);
                string fieldName = metadataReader.GetString(fieldDefinition.Name);
                FieldAttributes fieldAttributes = fieldDefinition.Attributes;
                TypeDescription fieldType = fieldDefinition.DecodeSignature(signatureDecoder, null);

                ConstantHandle constantHandle = fieldDefinition.GetDefaultValue();
                object constantValue = null;
                if (!constantHandle.IsNil) {
                    Constant constant = metadataReader.GetConstant(constantHandle);
                    constantValue = metadataReader.GetBlobReader(constant.Value).ReadConstant(constant.TypeCode);
                }

                FieldDescription field = new FieldDescription(type, fieldName, fieldAttributes, fieldType, constantValue);

                fields.Add(field);
                tokenStorage.StoreToken(fieldDefinitionHandle, field);
            }
            type.SetFields(fields);
        }

        private void LoadMethods(TypeDefinition typeDefinition, TypeDescription type, MetadataReader metadataReader, PEReader peReader, ITokenStorage tokenStorage, SignatureDecoder signatureDecoder) {
            List<MethodDescription> methods = new List<MethodDescription>();
            foreach (var methodDefinitionHandle in typeDefinition.GetMethods()) {
                var methodDefinition = metadataReader.GetMethodDefinition(methodDefinitionHandle);
                string methodName = metadataReader.GetString(methodDefinition.Name);
                MethodAttributes methodAttributes = methodDefinition.Attributes;
                MethodImplAttributes methodImplAttributes = methodDefinition.ImplAttributes;

                // Load method signature
                MethodSignature<TypeDescription> decodedSignature = methodDefinition.DecodeSignature(signatureDecoder, null);
                MethodSignatureDescription methodSignature = new MethodSignatureDescription(decodedSignature.Header, decodedSignature.GenericParameterCount, decodedSignature.RequiredParameterCount, decodedSignature.ReturnType, decodedSignature.ParameterTypes);

                // Load method body if we have one
                MethodBodyDescription methodBody = null;
                if (methodImplAttributes == MethodImplAttributes.IL && !methodAttributes.HasFlag(MethodAttributes.Abstract)) {
                    MethodBodyBlock methodBodyBlock = peReader.GetMethodBody(methodDefinition.RelativeVirtualAddress);

                    // Load locals
                    IList<TypeDescription> locals = null;
                    if (!methodBodyBlock.LocalSignature.IsNil) {
                        var localSignature = metadataReader.GetStandaloneSignature(methodBodyBlock.LocalSignature);
                        locals = localSignature.DecodeLocalSignature(signatureDecoder, null);
                    }

                    // Load exception regions
                    int exceptionRegionsLength = methodBodyBlock.ExceptionRegions.Length;
                    IList<MethodExceptionRegion> exceptionRegions = new MethodExceptionRegion[exceptionRegionsLength];
                    for (int i = 0; i < exceptionRegionsLength; i++) {
                        ExceptionRegion exceptionRegion = methodBodyBlock.ExceptionRegions[i];
                        TypeDescription catchType = null;
                        if (!exceptionRegion.CatchType.IsNil) {
                            catchType = tokenStorage.ResolveType(exceptionRegion.CatchType);
                        }
                        exceptionRegions[i] = new MethodExceptionRegion(exceptionRegion.Kind, catchType, exceptionRegion.HandlerOffset, exceptionRegion.HandlerLength, exceptionRegion.TryOffset, exceptionRegion.TryLength);
                    }

                    methodBody = new MethodBodyDescription(methodBodyBlock.MaxStack, methodBodyBlock.LocalVariablesInitialized, methodBodyBlock.GetILBytes(), locals, exceptionRegions);
                }

                MethodDescription method = new MethodDescription(type, methodName, methodAttributes, methodImplAttributes, methodSignature, methodBody);

                methods.Add(method);
                tokenStorage.StoreToken(methodDefinitionHandle, method);
            }
            type.SetMethods(methods);
        }

        private void LoadStrings(MetadataReader metadataReader, ITokenStorage tokenStorage) {
            Debug.Assert(metadataReader != null);
            Debug.Assert(tokenStorage != null);

            // We initialize the empty string here because we may reference it with a user string handle.
            StaticStorage.InitializeEmptyString();

            UserStringHandle handle = MetadataTokens.UserStringHandle(1);
            while (!handle.IsNil) {
                string s = metadataReader.GetUserString(handle);

                // We do not need to load empty strings.
                if (s == string.Empty) {
                    tokenStorage.StoreString(handle, StaticStorage.EmptyString);
                } else {
                    IntPtr ptr = StaticStorage.AllocateStaticStringFromLiteral(s);
                    tokenStorage.StoreString(handle, ptr);
                }

                handle = metadataReader.GetNextHandle(handle);
            }
        }
    }
}
