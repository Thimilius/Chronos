using Chronos.Exceptions;
using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Chronos.Execution {
    public partial class Interpreter {
        /// <summary>
        /// Throws an exception that is on the stack.
        /// 
        /// Stack: ..., obj -> ...
        /// </summary>
        private unsafe void InterpretThrow() {
            ExceptionObject* throwable = (ExceptionObject*)m_Frame.Pop<IntPtr>();
            // We are not allowed to throw a 'null'.
            ThrowOnInvalidPointer(throwable);

            VirtualMachine.ExceptionEngine.Throw(throwable);
        }

        /// <summary>
        /// Ends the finally clause of an exception region.
        /// 
        /// Stack: ... -> ...
        /// </summary>
        /// <param name="reader">A reference to the bytecode reader</param>
        private void InterpretEndFinally(ref CILReader reader) {
            if (m_LeaveInfoStack.Count == 0) {
                // We need to rethrow the exception.
                Exception dispatchedException = m_DispatchedException;
                m_DispatchedException = null;

#if TRACE_EXECUTION
                m_Tracer.TraceLine();
#endif

                throw dispatchedException;
            } else if (!MethodHasFinallyHandler(ref reader)) {
                LeaveInfo leaveInfo = m_LeaveInfoStack[--m_LeaveInfoStackOffset];
                m_LeaveInfoStack.RemoveAt(m_LeaveInfoStackOffset);
                reader.Seek(leaveInfo.Target);
            }
        }

        /// <summary>
        /// Leaves a protected code region.
        /// 
        /// Stack: ... -> ...
        /// </summary>
        /// <param name="offset">The offset from the current position to leave to</param>
        /// <param name="reader">A reference to the bytecode reader</param>
        private void InterpretLeave(int offset, ref CILReader reader) {
            // Leave always empties the operand stack.
            m_Frame.EmptyStack();

            int currentOffset = reader.Offset;
            int target = currentOffset + offset;
            m_LeaveInfoStack.Add(new LeaveInfo(currentOffset, target));
            m_LeaveInfoStackOffset++;

            // If we do not have a finally handler
            if (!MethodHasFinallyHandler(ref reader)) {
                m_LeaveInfoStack.RemoveAt(--m_LeaveInfoStackOffset);
                reader.Seek(target);
            }
        }

        private void InterpretRethrow() {
#if TRACE_EXECUTION
            m_Tracer.TraceLine();
#endif
            throw VirtualMachine.ExceptionEngine.LastThrowable;
        }

        /// <summary>
        /// Determines whether or not this method handles (catch or finally) a given exception and prepares the exectution.
        /// </summary>
        /// <param name="managedException">The managed exception to check</param>
        /// <param name="reader">A reference to the bytecode reader</param>
        /// <returns>True if the exception gets handled by this method otherwise false</returns>
        private unsafe bool MethodHandlesException(ManagedException managedException, ref CILReader reader) {
            ExceptionObject* throwable = managedException.Throwable;

            bool handlesException = false;

            IList<MethodExceptionRegion> exceptionRegions = m_Method.Body.ExceptionRegions;
            for (int i = 0; i < exceptionRegions.Count; i++) {
                MethodExceptionRegion exceptionRegion = exceptionRegions[i];
                int handlerOffset = 0;

                if (exceptionRegion.TryOffset <= reader.Offset && reader.Offset <= exceptionRegion.TryOffset + exceptionRegion.TryLength) {
                    if (exceptionRegion.Kind == ExceptionRegionKind.Catch) {
                        // Check if the catch handles the type of the exception that got thrown.
                        if (managedException.ThrowableType.IsInstanceOfBase(exceptionRegion.CatchType)) {
                            // Push throwable on the stack.
                            m_Frame.EmptyStack();
                            m_Frame.Push(RuntimeType.FromObjectReference(managedException.ThrowableType), (IntPtr)throwable);

                            handlerOffset = exceptionRegion.HandlerOffset;
                            handlesException = true;
                        }
                        
                    } else if (exceptionRegion.Kind == ExceptionRegionKind.Finally) {
                        m_Frame.EmptyStack();

                        m_DispatchedException = managedException;

                        handlerOffset = exceptionRegion.HandlerOffset;
                        handlesException = true;
                    } else {
                        // Faults and Filters are not supported.
                        throw new NotImplementedException();
                    }

                    if (handlesException) {
                        reader.Seek(handlerOffset);

                        while (m_LeaveInfoStack.Count > 0) {
                            m_LeaveInfoStack.RemoveAt(--m_LeaveInfoStackOffset);
                        }

                        break;
                    }
                }
            }

            return handlesException;
        }

        /// <summary>
        /// Determines wether or not a final handler should get executed and if so prepares the execution.
        /// </summary>
        /// <param name="reader">A reference to the bytecode reader</param>
        /// <returns>True if a final handler is present otherwise false</returns>
        private unsafe bool MethodHasFinallyHandler(ref CILReader reader) {
            IList<MethodExceptionRegion> exceptionRegions = m_Method.Body.ExceptionRegions;

            LeaveInfo leaveInfo = m_LeaveInfoStack[m_LeaveInfoStackOffset - 1];
            for (int i = leaveInfo.NextIndex; i < exceptionRegions.Count; i++) {
                MethodExceptionRegion exceptionRegion = exceptionRegions[i];

                int tryEndOffset = exceptionRegion.TryOffset + exceptionRegion.TryLength;
                if (exceptionRegion.TryOffset <= leaveInfo.Offset && leaveInfo.Offset <= tryEndOffset) {
                    if (exceptionRegion.Kind == ExceptionRegionKind.Finally) {
                        if (!(exceptionRegion.TryOffset <= leaveInfo.Target && leaveInfo.Target < tryEndOffset)) {
                            reader.Seek(exceptionRegion.HandlerOffset);
                            m_LeaveInfoStack[m_LeaveInfoStackOffset - 1] = new LeaveInfo(leaveInfo.Offset, leaveInfo.Target, i + 1);
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
