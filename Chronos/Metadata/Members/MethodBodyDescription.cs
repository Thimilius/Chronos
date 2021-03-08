using System.Collections.Generic;

namespace Chronos.Metadata {
    /// <summary>
    /// The metadata describing the body of a method.
    /// </summary>
    public class MethodBodyDescription {
        /// <summary>
        /// The maximum number of items on the stack.
        /// </summary>
        public int MaxStack { get; }
        /// <summary>
        /// Indicator for whether or not the local variables should be initialized.
        /// </summary>
        public bool InitLocals { get; }
        /// <summary>
        /// The bytes containing the bytecode of the method.
        /// </summary>
        public byte[] ILBytes { get; }
        /// <summary>
        /// The types of the locals of the method.
        /// </summary>
        public IList<TypeDescription> LocalTypes { get; }
        /// <summary>
        /// The exception regions of the method.
        /// </summary>
        public IList<MethodExceptionRegion> ExceptionRegions { get; }

        /// <summary>
        /// The locals of the method.
        /// </summary>
        public IList<Execution.LocalVariableInfo> Locals { get; private set; }
        /// <summary>
        /// The size in bytes needed for the local variables.
        /// </summary>
        public int LocalsSize { get; private set; }

        /// <summary>
        /// Gets the number of locals.
        /// </summary>
        public int LocalsCount => LocalTypes == null ? 0 : LocalTypes.Count;

        /// <summary>
        /// Constructs a new method body description.
        /// </summary>
        /// <param name="maxStack">The maximum number of items on teh stack</param>
        /// <param name="initLocals">Indicator for whether or not the local variables should be initialized</param>
        /// <param name="ilBytes">The bytes containing the bytecode of the method</param>
        /// <param name="locals">The types of the locals of the method</param>
        /// <param name="exceptionRegions">The exception regions of the method</param>
        public MethodBodyDescription(int maxStack, bool initLocals, byte[] ilBytes, IList<TypeDescription> locals, IList<MethodExceptionRegion> exceptionRegions) {
            MaxStack = maxStack;
            InitLocals = initLocals;
            ILBytes = ilBytes;
            LocalTypes = locals;
            ExceptionRegions = exceptionRegions;
        }

        /// <summary>
        /// Sets the local variable infos.
        /// </summary>
        /// <param name="locals">The local variable infos</param>
        public void SetLocals(Execution.LocalVariableInfo[] locals) {
            Locals = locals;
        }

        /// <summary>
        /// Sets the size in bytes needed for the local variables.
        /// </summary>
        /// <param name="localsSize">The size in bytes needed for the local variables</param>
        public void SetLocalsSize(int localsSize) {
            LocalsSize = localsSize;
        }
    }
}
