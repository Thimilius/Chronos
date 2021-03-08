using Chronos.Metadata;
using System.Reflection;

namespace Chronos.Loader {
    /// <summary>
    /// Interface for storing and retrieving assemblies.
    /// </summary>
    public interface IAssemblyStorage {
        /// <summary>
        /// Stores a given assembly with a unique name.
        /// </summary>
        /// <param name="name">The unique name identifying the assembly</param>
        /// <param name="module">The assembly to store</param>
        void StoreAssembly(AssemblyName name, ModuleDescription module);
        /// <summary>
        /// Retrieves an assembly by a given unique name.
        /// </summary>
        /// <param name="name">The unique name identifying the assembly</param>
        /// <returns>The assembly matching the unique name</returns>
        ModuleDescription RetrieveAssembly(AssemblyName name);
        /// <summary>
        /// Determines wether or not an assembly with a unique name is already stored.
        /// </summary>
        /// <param name="name">The unique name identifying the assembly</param>
        /// <returns>True if the assembly is already stored otherwise false</returns>
        bool ContainsAssembly(AssemblyName name);
    }
}
