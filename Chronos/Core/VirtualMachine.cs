using Chronos.Exceptions;
using Chronos.Execution;
using Chronos.Lib;
using Chronos.Loader;
using Chronos.Memory;
using Chronos.Metadata;
using Chronos.Model;
using Chronos.Tracing;
using System;
using System.Diagnostics;

namespace Chronos {
    /// <summary>
    /// The core of the virtual machine.
    /// Features explicitly missing and not implemented:
    ///    - Generics
    ///    - Raw Function pointers
    ///    - Custom Attributes
    ///    - Manifest Resources
    ///    - Module references
    ///    - P/Invoke
    /// </summary>
    public class VirtualMachine {
        /// <summary>
        /// Gets the execution engine.
        /// </summary>
        public static IExecutionEngine ExecutionEngine { get; private set; }
        /// <summary>
        /// Gets the garbage collector.
        /// </summary>
        public static IGarbageCollector GarbageCollector { get; private set; }
        /// <summary>
        /// Gets the exception engine.
        /// </summary>
        public static IExceptionEngine ExceptionEngine { get; private set; }

        /// <summary>
        /// Holds the virtual machine options.
        /// </summary>
        private readonly VirtualMachineOptions m_Options;

        /// <summary>
        /// Holds the tracer used for tracing the execution.
        /// </summary>
        private readonly ITracer m_ExecutionTracer;
        /// <summary>
        /// Holds the tracer used for diagnosing the execution.
        /// </summary>
        private readonly ITracer m_DiagnoseTracer;
        /// <summary>
        /// Holds the tracer used for tracing the garbage collector.
        /// </summary>
        private readonly ITracer m_GCTracer;
        /// <summary>
        /// A handle to the assembly storage.
        /// </summary>
        private readonly IAssemblyStorage m_Storage;
        /// <summary>
        /// A handle to the assembly loader.
        /// </summary>
        private readonly IAssemblyLoader m_Loader;

        /// <summary>
        /// A handle to the actual garbage collector implementation.
        /// </summary>
        private GarbageCollector m_GarbageCollector;
        /// <summary>
        /// A handle to the actual execution engine implementation.
        /// </summary>
        private ExecutionEngine m_ExecutionEngine;
        /// <summary>
        /// A handle to the executing assembly.
        /// </summary>
        private ModuleDescription m_ExecutingAssembly;

        /// <summary>
        /// Creates a new virtual machine with the given options.
        /// </summary>
        /// <param name="options">The virtual machine options to use</param>
        public VirtualMachine(VirtualMachineOptions options) {
            Debug.Assert(options != null);

            m_Options = options;

            // We should always get a proper string (We check for file existence later).
            Debug.Assert(options.PathToApplication != null);

            if (options.TracingMode.HasFlag(TracingMode.Execution)) {
                m_ExecutionTracer = new ConsoleTracer();
            } else {
                m_ExecutionTracer = new NonTracer();
            }

            if (options.TracingMode.HasFlag(TracingMode.Diagnostic)) {
                m_DiagnoseTracer = new ConsoleTracer();
            } else {
                m_DiagnoseTracer = new NonTracer();
            }

            if (options.TracingMode.HasFlag(TracingMode.GC)) {
                m_GCTracer = new ConsoleTracer();
            } else {
                m_GCTracer = new NonTracer();
            }

            m_Storage = new AssemblyStorage();
            m_Loader = new AssemblyLoader(m_ExecutionTracer, m_Storage);
        }

        /// <summary>
        /// Initializes the virtual machine loading in all required resources.
        /// </summary>
        public void Initialize() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            {
                // Setup the static storage first.
                // String literals found in the metadata are stored there.
                StaticStorage.PreLoad();

                // We want to load the memory info first so we can initalize the heap allocator
                // which is needed for the further loading process.
                ModuleMemoryInfo memoryInfo = m_Loader.LoadMemoryInfo(m_Options.PathToApplication);
                m_GarbageCollector = new GarbageCollector(m_GCTracer, memoryInfo.HeapSize);
                GarbageCollector = m_GarbageCollector;

                m_ExecutingAssembly = m_Loader.LoadExecutableAndReferences(m_Options.PathToApplication);

                m_ExecutionEngine = new InterpreterExecutionEngine(m_ExecutionTracer, memoryInfo.StackSize);
                ExecutionEngine = m_ExecutionEngine;
                ExceptionEngine = m_ExecutionEngine;

                Internals.Initialize();
                MetadataSystem.Initialize();
                StaticStorage.Initialize();
            }
            m_DiagnoseTracer.TraceColorLine(TracingConfig.DIAGNOSTIC_COLOR, "Load time: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
        }

        /// <summary>
        /// Runs the executing assembly.
        /// </summary>
        public unsafe void Run() {
            Stopwatch stopwatch = Stopwatch.StartNew();
            try {
                int exitCode = m_ExecutionEngine.RunEntryPoint(m_ExecutingAssembly.GetEntryPoint(), m_Options.ApplicationArguments);
                m_DiagnoseTracer.TraceColorLine(TracingConfig.DIAGNOSTIC_COLOR, "Execution time: {0}ms", stopwatch.Elapsed.TotalMilliseconds);
                m_DiagnoseTracer.TraceLine("Program has exited with code {0}", exitCode);
            } catch (ManagedException managedException) {
                // We need to handle any uncaught exception.
                Console.ForegroundColor = ConsoleColor.Red;

                // Try to display the exception message if it has one.
                ExceptionObject* throwable = managedException.Throwable;
                StringObject* str = throwable->Message;
                if (str != null && str->Length > 0) {
                    string message = new string(ObjectModel.GetStringBuffer(str), 0, str->Length);
                    Console.WriteLine("Unhandled exception. {0}: {1}", managedException.ThrowableType, message);
                } else {
                    Console.WriteLine("An unhandled exception of type '{0}' occurred in {1}.dll", managedException.ThrowableType, m_ExecutingAssembly.Name);
                }

                // Print the stack trace.
                foreach (IStackFrame stackFrame in managedException.StackFrames) {
                    Console.WriteLine("\tat {0}", stackFrame.Method.StackName);
                }
            }
        }

        /// <summary>
        /// Shutsdown the virtual machine and clears up all resources.
        /// </summary>
        public void Shutdown() {
            m_GarbageCollector.Shutdown();
            m_ExecutionEngine.Shutdown();

            StaticStorage.Shutdown();
            MetadataSystem.Shutdown();
        }
    }
}
