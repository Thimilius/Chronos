using Chronos.Execution;
using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Collections.Generic;

namespace Chronos.Exceptions {
    /// <summary>
    /// Represents a managed exception.
    /// </summary>
    public unsafe class ManagedException : Exception {
        /// <summary>
        /// The thrown exception object.
        /// </summary>
        public ExceptionObject* Throwable { get; }
        /// <summary>
        /// The type of the thrown exception object.
        /// </summary>
        public TypeDescription ThrowableType { get; }
        
        /// <summary>
        /// The stack frames the exception went through.
        /// </summary>
        private readonly Queue<IStackFrame> m_StackFrames;
        /// <summary>
        /// The stack frames the exception went through.
        /// </summary>
        public IEnumerable<IStackFrame> StackFrames => m_StackFrames;

        /// <summary>
        /// Constructs a new managed exception.
        /// </summary>
        /// <param name="throwable">The thrown exception object</param>
        /// <param name="throwableType">The type of the thrown exception object</param>
        public ManagedException(ExceptionObject* throwable, TypeDescription throwableType) {
            Throwable = throwable;
            ThrowableType = throwableType;
            m_StackFrames = new Queue<IStackFrame>();
        }

        /// <summary>
        /// Pushes a new stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame to push</param>
        public void PushStackFrame(IStackFrame stackFrame) {
            m_StackFrames.Enqueue(stackFrame);
        }
    }
}
