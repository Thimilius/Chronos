using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Collections.Immutable;
using System.Reflection.Metadata;

namespace Chronos.Loader {
    public class SignatureDecoderGenericContext {

    }

    public class SignatureDecoder : ISignatureTypeProvider<TypeDescription, SignatureDecoderGenericContext> {
        private readonly ITokenResolver m_TokenResolver;

        public SignatureDecoder(ITokenResolver tokenResolver) {
            m_TokenResolver = tokenResolver;
        }

        public TypeDescription GetPinnedType(TypeDescription elementType) {
            // NOTE: We should probably save the fact that it is a pinned type.
            // Right now it does not matter, as we do not have a moving garbage collector.
            return elementType;
        }

        public TypeDescription GetByReferenceType(TypeDescription elementType) {
            return MetadataSystem.GetOrCreateByReferenceType(elementType);
        }

        public TypeDescription GetPointerType(TypeDescription elementType) {
            return MetadataSystem.GetOrCreatePointerType(elementType);
        }

        public TypeDescription GetSZArrayType(TypeDescription elementType) {
            return MetadataSystem.GetOrCreateArrayType(elementType, 1);
        }

        public TypeDescription GetArrayType(TypeDescription elementType, ArrayShape shape) {
            return MetadataSystem.GetOrCreateArrayType(elementType, shape.Rank);
        }

        public TypeDescription GetPrimitiveType(PrimitiveTypeCode typeCode) {
            return typeCode switch
            {
                PrimitiveTypeCode.Void => MetadataSystem.VoidType,
                PrimitiveTypeCode.Boolean => MetadataSystem.BooleanType,
                PrimitiveTypeCode.Char => MetadataSystem.CharType,
                PrimitiveTypeCode.SByte => MetadataSystem.SByteType,
                PrimitiveTypeCode.Byte => MetadataSystem.ByteType,
                PrimitiveTypeCode.Int16 => MetadataSystem.Int16Type,
                PrimitiveTypeCode.UInt16 => MetadataSystem.UInt16Type,
                PrimitiveTypeCode.Int32 => MetadataSystem.Int32Type,
                PrimitiveTypeCode.UInt32 => MetadataSystem.UInt32Type,
                PrimitiveTypeCode.Int64 => MetadataSystem.Int64Type,
                PrimitiveTypeCode.UInt64 => MetadataSystem.UInt64Type,
                PrimitiveTypeCode.Single => MetadataSystem.SingleType,
                PrimitiveTypeCode.Double => MetadataSystem.DoubleType,
                PrimitiveTypeCode.IntPtr => MetadataSystem.IntPtrType,
                PrimitiveTypeCode.UIntPtr => MetadataSystem.UIntPtrType,
                PrimitiveTypeCode.Object => MetadataSystem.ObjectType,
                PrimitiveTypeCode.String => MetadataSystem.StringType,

                _ => throw new InvalidOperationException(),
            };
        }

        public TypeDescription GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind) {
            return m_TokenResolver.ResolveType(handle);
        }

        public TypeDescription GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind) {
            return m_TokenResolver.ResolveType(handle);
        }

        public TypeDescription GetFunctionPointerType(MethodSignature<TypeDescription> signature) {
            throw new NotImplementedException();
        }

        public TypeDescription GetGenericMethodParameter(SignatureDecoderGenericContext genericContext, int index) {
            return new SignatureMethodVariableDescription(null, index);
        }

        public TypeDescription GetGenericTypeParameter(SignatureDecoderGenericContext genericContext, int index) {
            return new SignatureTypeVariableDescription(null, index);
        }

        public TypeDescription GetModifiedType(TypeDescription modifier, TypeDescription unmodifiedType, bool isRequired) {
            throw new NotImplementedException();
        }

        public TypeDescription GetTypeFromSpecification(MetadataReader reader, SignatureDecoderGenericContext genericContext, TypeSpecificationHandle handle, byte rawTypeKind) {
            throw new NotImplementedException();
        }

        public TypeDescription GetGenericInstantiation(TypeDescription genericType, ImmutableArray<TypeDescription> typeArguments) {
            return MetadataSystem.GetOrCreateInstantiatedType(genericType, new Instantiation(typeArguments));
        }
    }
}
