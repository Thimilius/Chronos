using Chronos.Tracing;
using McMaster.Extensions.CommandLineUtils;
using System;

namespace Chronos {
    internal static class Program {
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
            // Setup and define available command line options
            var app = new CommandLineApplication() {
                FullName = "Chronos",
            };
            app.HelpOption().Description = "Display help.";
            app.VersionOption("-v|--version", "1.0.0").Description = "Display version.";
            var traceModeOption = app.Option("-t|--trace", "Specifies the tracing mode.", CommandOptionType.SingleValue)
                .Accepts(v => v.Enum<TracingMode>());
            var applicationPathArgument = app.Argument("path-to-application", "The path to an application .dll file to execute.")
                .IsRequired()
                .Accepts(v => v.ExistingFile());
            var applicationArgumentsArgument = app.Argument("application-arguments", "The arguments the application should recieve.", true);
            
            bool success = false;
            options = null;
            VirtualMachineOptions result = null;
            app.OnExecute(() => {
                success = true;

                result = new VirtualMachineOptions() {
                    PathToApplication = applicationPathArgument.Value,
                    ApplicationArguments = applicationArgumentsArgument.Values,
                    TracingMode = Enum.Parse<TracingMode>(traceModeOption.Value())
                };
            });
            
            // We want to directly display the help option if no arguments get passed.
            if (args.Length == 0) {
                app.ShowHelp();
            } else {
                try {
                    app.Execute(args);
                    options = result;
                } catch (Exception e) {
                    PrintException(e);
                }
            }

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
