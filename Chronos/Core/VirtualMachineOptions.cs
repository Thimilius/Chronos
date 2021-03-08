using Chronos.Tracing;
using System.Collections.Generic;

namespace Chronos {
    /// <summary>
    /// Options for the virtual machine.
    /// </summary>
    public class VirtualMachineOptions {
        /// <summary>
        /// The path to the application .dll to run.
        /// </summary>
        public string PathToApplication { get; set; }
        /// <summary>
        /// The arguments to the application.
        /// </summary>
        public IList<string> ApplicationArguments { get; set; }
        /// <summary>
        /// The tracing mode used.
        /// </summary>
        public TracingMode TracingMode { get; set; }
    }
}
