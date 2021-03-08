using Chronos.Metadata;
using Chronos.Model;
using Chronos.Tracing;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace Chronos.Loader {
    // The loading process is implemented in the following way:
    // For the runtime to function properly it is always assumed that every program
    // needs the core system library 'Chronos.System' and therefore has a reference to it.
    // The core system library gets loaded in completley before every other assembly.
    // That ensures that the special system types are setup in order for other types to reference them.
    // That is possible because the library does not depend on any other assembly.
    // The executing assembly and all the references that are needed are loaded in next in two seperate phases:
    //     1. Load in all existing types
    //     2. Resolve type references and setup all members (methods, fields)
    //     3. Resolve remaining member references
    // The resolving of type and member references in a assembly at load time ensures that the execution does not have to deal with it
    public partial class AssemblyLoader : IAssemblyLoader {
        private delegate ModuleDescription AssemblyPassLoader(string executableDirectory, PEReader peReader, MetadataReader metadataReader);

        private const string SYSTEM_LIBRARY_NAME = "Chronos.System";

        private readonly ITracer m_Tracer;
        private readonly IAssemblyStorage m_Storage;

        public AssemblyLoader(ITracer tracer, IAssemblyStorage storage) {
            m_Tracer = tracer;
            m_Storage = storage;
        }

        public ModuleMemoryInfo LoadMemoryInfo(string filePath) {
            Debug.Assert(filePath != null);

            if (!File.Exists(filePath)) {
                throw new DllNotFoundException(filePath);
            }

            PEReader peReader = null;
            try {
                peReader = new PEReader(File.OpenRead(filePath));
                MetadataReader metadataReader = peReader.GetMetadataReader();

                // Make sure the assembly we want to load is actually executable
                if (!IsValidAssembly(peReader)) {
                    throw new BadImageFormatException("Trying to load an invalid assembly");
                }

                return ResolveMemoryInfo(peReader);
            } finally {
                peReader.Dispose();
            }
        }

        public ModuleDescription LoadExecutableAndReferences(string filePath) {
            Debug.Assert(filePath != null);

            if (!File.Exists(filePath)) {
                throw new DllNotFoundException(filePath);
            }

            // Before we load the actual executable we load the core system library
            LoadSystemLibrary();

            PEReader peReader = null;
            try {
                peReader = new PEReader(File.OpenRead(filePath));
                MetadataReader metadataReader = peReader.GetMetadataReader();

                // Make sure the assembly we want to load is actually executable
                if (!IsValidExecutableAssembly(peReader)) {
                    throw new BadImageFormatException("Trying to load an invalid assembly");
                }

                // Get the directory in which the executable resides in
                string executableDirectory = Path.GetDirectoryName(Path.GetFullPath(filePath));

                ModuleDescription result = DoAssemblyLoadFirstPassWithReferences(executableDirectory, peReader, metadataReader);
                DoAssemblyLoadSecondPassWithReferences(executableDirectory, peReader, metadataReader);
                DoAssemblyLoadThirdPassWithReferences(executableDirectory, peReader, metadataReader);
                result.SetEntryPoint(MetadataTokens.EntityHandle(peReader.PEHeaders.CorHeader.EntryPointTokenOrRelativeVirtualAddress));

                m_Tracer.TraceColorLine(TracingConfig.LOADING_COLOR, "Loaded: '{0}'", Path.GetFullPath(filePath));

                return result;
            } finally {
                peReader.Dispose();
            }
        }

        public void LoadSystemLibrary() {
            // The system library 'Chronos.System' is assumed to be distributed alongside us.
            string systemLibraryBasePath = AppDomain.CurrentDomain.BaseDirectory;
            string systemLibraryPath = Path.Combine(systemLibraryBasePath, SYSTEM_LIBRARY_NAME + ".dll");

            if (!File.Exists(systemLibraryPath)) {
                throw new DllNotFoundException($"System library 'Chronos.System' can not be found at path: '{systemLibraryPath}'");
            }

            PEReader peReader = null;
            try {
                peReader = new PEReader(File.OpenRead(systemLibraryPath));
                MetadataReader metadataReader = peReader.GetMetadataReader();

                if (!IsValidAssemblyLibrary(peReader)) {
                    throw new BadImageFormatException("Trying to load an invalid assembly");
                }
                
                AssemblyName name = metadataReader.GetAssemblyDefinition().GetAssemblyName();
                ModuleDescription module = new ModuleDescription(name.Name, ResolveMemoryInfo(peReader), ResolveTokenCountInfo(metadataReader));

                // Ensure the system library does not have any references to other assemblies
                Debug.Assert(metadataReader.AssemblyReferences.Count == 0);

                DoAssemblyLoadFirstPass(metadataReader, module);
                // We cache the special system types for further loading and actual execution
                MetadataSystem.ResolveSpecialSystemTypes(metadataReader, module);
                DoAssemblyLoadSecondPass(peReader, metadataReader, module);
                DoAssemblyLoadThirdPass(metadataReader, module);

                m_Storage.StoreAssembly(name, module);


                m_Tracer.TraceColorLine(TracingConfig.LOADING_COLOR, "Loaded: '{0}'", systemLibraryPath);
            } finally {
                peReader.Dispose();
            }
        }

        private void DoAssemblyLoadPassWithReferences(string executableDirectory, AssemblyName referenceName, AssemblyPassLoader passLoader) {
            Debug.Assert(executableDirectory != null);
            Debug.Assert(referenceName != null);

            // References are assumed to be placed next to the executing assembly.
            // We do not have a central place (like the GAC) where we can look for assemblies.
            string filePath = Path.Combine(executableDirectory, referenceName.Name + ".dll");

            if (!File.Exists(filePath)) {
                throw new DllNotFoundException(filePath);
            }

            PEReader peReader = null;
            try {
                peReader = new PEReader(File.OpenRead(filePath));
                MetadataReader metadataReader = peReader.GetMetadataReader();

                // Make sure the assembly we want to load is actually executable
                if (!IsValidAssemblyLibrary(peReader)) {
                    throw new BadImageFormatException("Trying to load an invalid assembly");
                }

                passLoader(executableDirectory, peReader, peReader.GetMetadataReader());
            } finally {
                peReader.Dispose();
            }
        }

        private ModuleMemoryInfo ResolveMemoryInfo(PEReader peReader) {
            var header = peReader.PEHeaders.PEHeader;
            return new ModuleMemoryInfo((int)header.SizeOfStackReserve, (int)header.SizeOfHeapReserve);
        }

        private ModuleTokenCountInfo ResolveTokenCountInfo(MetadataReader metadataReader) {
            int stringCount = 0;
            UserStringHandle handle = MetadataTokens.UserStringHandle(1);
            while (!handle.IsNil) {
                string s = metadataReader.GetUserString(handle);

                // We do not need to count empty strings
                if (s != string.Empty) {
                    stringCount++;
                }

                handle = metadataReader.GetNextHandle(handle);
            }

            return new ModuleTokenCountInfo() {
                TypeDefinitionsCount = metadataReader.GetTableRowCount(TableIndex.TypeDef),
                TypeSpecificationsCount = metadataReader.GetTableRowCount(TableIndex.TypeSpec),
                TypeReferencesCount = metadataReader.GetTableRowCount(TableIndex.TypeRef),
                MethodDefinitionsCount = metadataReader.GetTableRowCount(TableIndex.MethodDef),
                MethodSpecificationsCount = metadataReader.GetTableRowCount(TableIndex.MethodSpec),
                FieldDefinitionsCount = metadataReader.GetTableRowCount(TableIndex.Field),
                MemberReferencesCount = metadataReader.GetTableRowCount(TableIndex.MemberRef),
                StringCount = stringCount,
            };
        }

        private bool IsValidExecutableAssembly(PEReader peReader) {
            if (!IsValidAssembly(peReader)) {
                return false;
            }
            
            // Check for a valid entry point
            if (peReader.PEHeaders.CorHeader.EntryPointTokenOrRelativeVirtualAddress == 0) {
                throw new EntryPointNotFoundException();
            }

            if (peReader.PEHeaders.IsDll && !peReader.PEHeaders.IsExe) {
                return false;
            }

            return true;
        }

        private bool IsValidAssemblyLibrary(PEReader peReader) {
            if (!IsValidAssembly(peReader)) {
                return false;
            }

            if (!peReader.PEHeaders.IsDll) {
                return false;
            }

            return true;
        }

        private bool IsValidAssembly(PEReader peReader) {
            // NOTE: For now we want our assembly to always be in x64.
            if (peReader.PEHeaders.CoffHeader.Machine != Machine.Amd64) {
                throw new NotSupportedException("Only assemblies that target x64 are supported");
            }

            return peReader.HasMetadata;
        }   
    }
}
