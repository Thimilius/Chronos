using Chronos.Metadata;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Chronos.Loader {
    /// <summary>
    /// Stores and retrieves assemblies.
    /// </summary>
    public class AssemblyStorage : IAssemblyStorage {
        /// <summary>
        /// Holds a list of all assemblies identified by their name.
        /// </summary>
        private readonly Dictionary<string, ModuleDescription> m_Assemblies;

        /// <summary>
        /// Constructs a new assembly storage.
        /// </summary>
        public AssemblyStorage() {
            m_Assemblies = new Dictionary<string, ModuleDescription>();
        }

        /// <summary>
        /// <inheritdoc cref="IAssemblyStorage.StoreAssembly(AssemblyName, ModuleDescription)"/>
        /// </summary>
        public void StoreAssembly(AssemblyName name, ModuleDescription module) {
            Debug.Assert(!m_Assemblies.ContainsKey(name.FullName));

            m_Assemblies[name.FullName] = module;
        }

        /// <summary>
        /// <inheritdoc cref="IAssemblyStorage.RetrieveAssembly(AssemblyName)"/>
        /// </summary>
        public ModuleDescription RetrieveAssembly(AssemblyName name) {
            Debug.Assert(m_Assemblies.ContainsKey(name.FullName));
            
            return m_Assemblies[name.FullName];
        }

        /// <summary>
        /// <inheritdoc cref="IAssemblyStorage.ContainsAssembly(AssemblyName)"/>
        /// </summary>
        public bool ContainsAssembly(AssemblyName name) {
            return m_Assemblies.ContainsKey(name.FullName);
        }
    }
}
