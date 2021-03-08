using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing an instantiated type.
    /// </summary>
    public class InstantiatedTypeDescription : TypeDescription {
        /// <summary>
        /// The generic type of the instantiated type.
        /// </summary>
        public TypeDescription GenericType { get; }
        /// <summary>
        /// <inheritdoc cref="TypeDescription.Instantiation"/>
        /// </summary>
        public override Instantiation Instantiation { get; }

        /// <summary>
        /// Constructs a new instanitated type.
        /// </summary>
        /// <param name="module">The module of the instantiated type</param>
        /// <param name="genericType">The generic type of the instantiated type</param>
        /// <param name="instantiation">The instantiation of the instantiated type</param>
        public InstantiatedTypeDescription(ModuleDescription module, TypeDescription genericType, Instantiation instantiation) : base(module) {
            Debug.Assert(!(genericType is InstantiatedTypeDescription));

            GenericType = genericType;
            Instantiation = instantiation;

            SetMethods(new List<MethodDescription>());
            SetFields(new List<FieldDescription>());
            SetMethodImplementations(new Dictionary<MethodDescription, MethodDescription>());
            SetInterfaces(new List<TypeDescription>());

            Name = genericType.Name;
            Namespace = genericType.Namespace;
        }

        /// <summary>
        /// <inheritdoc cref="TypeDescription.InstantiateSignature(Instantiation, Instantiation)"/>
        /// </summary>
        public override TypeDescription InstantiateSignature(Instantiation typeInstantiation, Instantiation methodInstantiation) {
            TypeDescription[] clone = null;

            for (int i = 0; i < Instantiation.Length; i++) {
                TypeDescription uninstantiated = Instantiation[i];
                TypeDescription instantiated = uninstantiated.InstantiateSignature(typeInstantiation, methodInstantiation);
                if (instantiated != uninstantiated) {
                    if (clone == null) {
                        clone = new TypeDescription[Instantiation.Length];
                        for (int j = 0; j < clone.Length; j++) {
                            clone[j] = Instantiation[j];
                        }
                    }
                    clone[i] = instantiated;
                }
            }

            //return (clone == null) ? this : MetadataSystem.GetInstantiatedType(GenericType, new Instantiation(ImmutableArray.Create(clone)));
            return null;
        }
    }
}
