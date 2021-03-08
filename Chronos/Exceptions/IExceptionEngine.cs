using Chronos.Metadata;
using Chronos.Model;
using System;

namespace Chronos.Exceptions {
    /// <summary>
    /// An interface for interacting with exceptions.
    /// </summary>
    public interface IExceptionEngine {
        /// <summary>
        /// Holds the last thrown managed exception.
        /// </summary>
        ManagedException LastThrowable { get; }

        /// <summary>
        /// Throws a managed exception with the given exception object.
        /// </summary>
        /// <param name="throwable">The exception object to throw</param>
        unsafe void Throw(ExceptionObject* throwable);
        /// <summary>
        /// Throws a managed exception of a given type.
        /// </summary>
        /// <param name="type">The type of the exception</param>
        void Throw(TypeDescription type);

        /// <summary>
        /// Throws a managed NullReferenceException.
        /// </summary>
        public void ThrowNullReferenceException();
        /// <summary>
        /// Throws a managed InvalidCastException.
        /// </summary>
        public void ThrowInvalidCastException();
        /// <summary>
        /// Throws a managed IndexOutOfRangeException.
        /// </summary>
        public void ThrowIndexOutOfRangeException();
        /// <summary>
        /// Throws a managed ArithmeticException.
        /// </summary>
        public void ThrowNotFiniteNumberException();
        /// <summary>
        /// Throws a managed DivideByZeroException.
        /// </summary>
        public void ThrowDivideByZeroException();

        /// <summary>
        /// Throw an exception if a given pointer is invalid.
        /// </summary>
        /// <param name="pointer">The pointer to check</param>
        unsafe void ThrowOnInvalidPointer(void* pointer);
        /// <summary>
        /// Throw an exception if a given pointer is invalid.
        /// </summary>
        /// <param name="pointer">The pointer to check</param>
        void ThrowOnInvalidPointer(IntPtr pointer);
    }
}
