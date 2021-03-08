using Chronos.Exceptions;
using Chronos.Metadata;
using Chronos.Model;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Chronos.Execution {
    public partial class ExecutionEngine : IExceptionEngine {
        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.LastThrowable"/>
        /// </summary>
        public ManagedException LastThrowable { get; private set; }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.Throw(ExceptionObject*)"/>
        /// </summary>
        public unsafe void Throw(ExceptionObject* throwable) {
#if TRACE_EXECUTION
            Tracer.TraceLine();
#endif
            TypeDescription throwableType = throwable->Base.Type;
            ManagedException managedException = new ManagedException(throwable, throwableType);
            managedException.PushStackFrame(m_StackFrames.Peek());

            LastThrowable = managedException;

            throw managedException;
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.Throw(TypeDescription)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Throw(TypeDescription type) {
            Debug.Assert(type.IsInstanceOfBase(MetadataSystem.ExceptionType));

            ExceptionObject* throwable = (ExceptionObject*)VirtualMachine.GarbageCollector.AllocateNewObject(type);
            Throw(throwable);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowNullReferenceException"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowNullReferenceException() {
            Throw(MetadataSystem.NullReferenceExceptionType);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowInvalidCastException"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowInvalidCastException() {
            Throw(MetadataSystem.InvalidCastExceptionType);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowIndexOutOfRangeException"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIndexOutOfRangeException() {
            Throw(MetadataSystem.IndexOutOfRangeExceptionType);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowNotFiniteNumberException"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowNotFiniteNumberException() {
            Throw(MetadataSystem.NotFiniteNumberExceptionType);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowDivideByZeroException"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowDivideByZeroException() {
            Throw(MetadataSystem.DivideByZeroExceptionType);
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowOnInvalidPointer(void*)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ThrowOnInvalidPointer(void* pointer) {
            if (pointer == null) {
                Throw(MetadataSystem.NullReferenceExceptionType);
            }
        }

        /// <summary>
        /// <inheritdoc cref="IExceptionEngine.ThrowOnInvalidPointer(IntPtr)"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowOnInvalidPointer(IntPtr pointer) {
            if (pointer == IntPtr.Zero) {
                Throw(MetadataSystem.NullReferenceExceptionType);
            }
        }
    }
}
