using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace Chronos.Metadata {
    /// <summary>
    /// Handles the metadata at load and runtime.
    /// </summary>
    public static partial class MetadataSystem {
        /// <summary>
        /// Holds a list of all loaded types.
        /// </summary>
        private static readonly List<TypeDescription> s_Types = new List<TypeDescription>();
        /// <summary>
        /// Gets a collection of all loaded types.
        /// </summary>
        public static IEnumerable<TypeDescription> Types => s_Types;
        /// <summary>
        /// Holds a list of all array types.
        /// </summary>
        private static readonly List<ArrayTypeDescription> s_ArrayTypes = new List<ArrayTypeDescription>();
        /// <summary>
        /// Holds a list of all pointer types.
        /// </summary>
        private static readonly List<PointerTypeDescription> s_PointerTypes = new List<PointerTypeDescription>();
        /// <summary>
        /// Holds a list of all by reference types.
        /// </summary>
        private static readonly List<ByReferenceTypeDescription> s_ByReferenceTypes = new List<ByReferenceTypeDescription>();
        /// <summary>
        /// Holds a list of all instantiated types.
        /// </summary>
        private static readonly List<InstantiatedTypeDescription> s_InstantiatedTypes = new List<InstantiatedTypeDescription>();

        /// <summary>
        /// Gets an array type for a given element type.
        /// </summary>
        /// <param name="elementType">The element type of the array type</param>
        /// <returns>The array type with the element type</returns>
        public static ArrayTypeDescription GetArrayType(TypeDescription elementType, int rank) {
            Debug.Assert(s_ArrayTypes.Any(a => a.ParameterType == elementType && a.Rank == rank));

            // NOTE: This may not be very efficient.
            return s_ArrayTypes.First(a => a.ParameterType == elementType && a.Rank == rank);
        }

        /// <summary>
        /// Gets or creates an array type with a given element type.
        /// </summary>
        /// <param name="elementType">The element type of the array type</param>
        /// <param name="rank">The rank of the array type</param>
        /// <returns>The array type with the element type</returns>
        public static ArrayTypeDescription GetOrCreateArrayType(TypeDescription elementType, int rank) {
            // Check wether or not we already have the type stored.
            if (!s_ArrayTypes.Any(a => a.ParameterType == elementType && a.Rank == rank)) {
                ArrayTypeDescription arrayType = new ArrayTypeDescription(elementType, rank);

                // All arrays inherit from the corresponding System.Array base type.
                arrayType.SetBaseType(ArrayType);

                StoreType(arrayType);
                s_ArrayTypes.Add(arrayType);

                // For multidimensional arrays we have to prepare the methods for the type manually.
                if (arrayType.IsMDArray) {
                    List<MethodDescription> methods = new List<MethodDescription>();

                    // NOTE: Theoretically there are two seperate constructors.
                    // One that takes in the number of elements in each dimension
                    // and another that takes as a pair the lower bound and number of elements in each dimension.
                    // Since C# only permits arrays with a zero lower bound, the second constructor is never used.

                    TypeDescription[] indexArguments = new TypeDescription[rank];
                    for (int i = 0; i < rank; i++) {
                        indexArguments[i] = Int32Type;
                    }
                    ImmutableArray<TypeDescription> indexParameterTypes = ImmutableArray.Create(indexArguments);
                    SignatureHeader signatureHeader = new SignatureHeader(SignatureKind.Method, SignatureCallingConvention.Default, SignatureAttributes.Instance);

                    // .ctor
                    {
                        TypeDescription returnType = VoidType;
                        MethodSignatureDescription signature = new MethodSignatureDescription(signatureHeader, 0, rank, returnType, indexParameterTypes);
                        MethodDescription method =  new MethodDescription(arrayType, ".ctor", MethodAttributes.Public, MethodImplAttributes.Runtime, signature, null);
                        methods.Add(method);
                    }

                    // Get
                    {
                        TypeDescription returnType = elementType;
                        MethodSignatureDescription signature = new MethodSignatureDescription(signatureHeader, 0, rank, returnType, indexParameterTypes);
                        MethodDescription method = new MethodDescription(arrayType, "Get", MethodAttributes.Public, MethodImplAttributes.Runtime, signature, null);
                        methods.Add(method);
                    }

                    // Set
                    {
                        TypeDescription returnType = VoidType;
                        var builder =  ImmutableArray.CreateBuilder<TypeDescription>(rank + 1);
                        builder.AddRange(indexArguments);
                        builder.AddRange(elementType);
                        MethodSignatureDescription signature = new MethodSignatureDescription(signatureHeader, 0, rank + 1,  returnType, builder.ToImmutable());
                        MethodDescription method = new MethodDescription(arrayType, "Set", MethodAttributes.Public, MethodImplAttributes.Runtime, signature, null);
                        methods.Add(method);
                    }

                    // Address
                    {
                        TypeDescription returnType = GetOrCreateByReferenceType(elementType);
                        MethodSignatureDescription signature = new MethodSignatureDescription(signatureHeader, 0, rank, returnType, indexParameterTypes);
                        MethodDescription method = new MethodDescription(arrayType, "Address", MethodAttributes.Public, MethodImplAttributes.Runtime, signature, null);
                        methods.Add(method);
                    }

                    arrayType.SetMethods(methods);
                }

                return arrayType;
            } else {
                return GetArrayType(elementType, rank);
            }
        }

        /// <summary>
        /// Gets or creates a pointer type with a given parameter type.
        /// </summary>
        /// <param name="parameterType">The parameter type of the pointer type</param>
        /// <returns>The pointer type with the parameter type</returns>
        public static PointerTypeDescription GetOrCreatePointerType(TypeDescription parameterType) {
            // Check wether or not we already have the type stored.
            if (!s_PointerTypes.Any(a => a.ParameterType == parameterType)) {
                PointerTypeDescription pointerType = new PointerTypeDescription(parameterType);

                StoreType(pointerType);
                s_PointerTypes.Add(pointerType);

                return pointerType;
            } else {
                return s_PointerTypes.First(a => a.ParameterType == parameterType);
            }
        }

        /// <summary>
        /// Gets or creates a by reference type with a given parameter type.
        /// </summary>
        /// <param name="parameterType">The parameter type of the by reference type</param>
        /// <returns>The by reference type with the parameter type</returns>
        public static ByReferenceTypeDescription GetOrCreateByReferenceType(TypeDescription parameterType) {
            // Check wether or not we already have the type stored.
            if (!s_ByReferenceTypes.Any(a => a.ParameterType == parameterType)) {
                ByReferenceTypeDescription byReferenceType = new ByReferenceTypeDescription(parameterType);

                StoreType(byReferenceType);
                s_ByReferenceTypes.Add(byReferenceType);

                return byReferenceType;
            } else {
                return s_ByReferenceTypes.First(a => a.ParameterType == parameterType);
            }
        }

        /// <summary>
        /// Gets or creates an instantiated type with a given generic type and instantiation.
        /// </summary>
        /// <param name="genericType">The generic type</param>
        /// <param name="instantiation">The instantiation</param>
        /// <returns>The instantiated type</returns>
        public static InstantiatedTypeDescription GetOrCreateInstantiatedType(TypeDescription genericType, Instantiation instantiation) {
            if (!s_InstantiatedTypes.Any(i => i.GenericType == genericType && i.Instantiation.Equals(instantiation))) {
                InstantiatedTypeDescription instantiatedType = new InstantiatedTypeDescription(genericType.OwningModule, genericType, instantiation);

                s_InstantiatedTypes.Add(instantiatedType);

                return instantiatedType;
            } else {
                return s_InstantiatedTypes.First(i => i.GenericType == genericType && i.Instantiation.Equals(instantiation));
            }
        }

        /// <summary>
        /// Stores a given type.
        /// </summary>
        /// <param name="type">The type to store</param>
        public static void StoreType(TypeDescription type) {
            s_Types.Add(type);
        }

        /// <summary>
        /// Shutsdown the type system and clears all its resources.
        /// </summary>
        public static void Shutdown() {
            foreach (TypeDescription type in s_Types) {
                foreach (MethodDescription method in type.Methods) {
                    method.Handle.Free();
                }
                type.Handle.Free();
            }
        }
    }
}
