using System;
using System.Collections.Generic;
using CommandLine;
using Chronos.Tracing;
using System.Linq;

namespace Chronos {
    internal static class Program {
        /// <summary>
        /// The command line options for the application.
        /// </summary>
        public class CommandLineOptions {
            [Option(shortName: 't', longName: "trace", Required = false, HelpText = "Specifies the tracing mode.", Default = TracingMode.None)]
            public TracingMode TracingMode { get; set; }

            [Value(0, Required = true, MetaName = "path-to-application", HelpText = "The path to an application .dll file to execute.")]
            public string PathToApplication { get; set; }
            
            [Option(longName: "application-arguments", Required = false, HelpText = "The arguments the application should recieve.")]
            public IEnumerable<string> ApplicationArguments { get; set; }
        }

        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        private static void Main(string[] args) {
            bool success = ParseCommandLineArguments(args, out VirtualMachineOptions options);
            if (!success) {
                return;
            }
            
#if !DEBUG
            try {
#endif
            // Run the virtual machine with the parsed options.
            var vm = new VirtualMachine(options);
                vm.Initialize();
                vm.Run();
                vm.Shutdown();
#if !DEBUG
            } catch(Exception e) {
                PrintException(e);
            }
#endif

#if DEBUG
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// Parses the command line arguments into virtual machine options.
        /// </summary>
        /// <param name="args">The arguments passed to the program</param>
        /// <param name="options">The parsed options to be returned</param>
        /// <returns>True if the parsing was successfull otherwise false</returns>
        private static bool ParseCommandLineArguments(string[] args, out VirtualMachineOptions options) {
            bool success = false;

            options = Parser.Default.ParseArguments<CommandLineOptions>(args).MapResult(opt => {
                success = true;
                return new VirtualMachineOptions() {
                    TracingMode = opt.TracingMode,
                    PathToApplication = opt.PathToApplication,
                    ApplicationArguments = opt.ApplicationArguments.ToList(),
                };
            },
            errors => {
                success = false;
                return new VirtualMachineOptions();
            });

            return success;
        }

        /// <summary>
        /// Prints an exception to the console.
        /// </summary>
        /// <param name="e">The exception to print</param>
        private static void PrintException(Exception e) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message + ".");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
