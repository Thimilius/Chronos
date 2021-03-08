using Chronos.Metadata;

namespace Chronos.Loader {
    /// <summary>
    /// Interface for loading in assemblies.
    /// </summary>
    public interface IAssemblyLoader {
        /// <summary>
        /// Loads in the memory information from a given path to an assembly.
        /// </summary>
        /// <param name="filePath">The path to the assembly</param>
        /// <returns>The memory information for the assembly</returns>
        ModuleMemoryInfo LoadMemoryInfo(string filePath);
        /// <summary>
        /// Loads in an executing assembly at a given path and all its references.
        /// </summary>
        /// <param name="filePath">The path to the executable assembly</param>
        /// <returns>The loaded executable assembly</returns>
        ModuleDescription LoadExecutableAndReferences(string filePath);
    }
}
