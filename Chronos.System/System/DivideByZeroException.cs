namespace System {
    public class DivideByZeroException : ArithmeticException {
        public DivideByZeroException() { }

        public DivideByZeroException(string message) : base(message) { }

        public DivideByZeroException(string message, Exception innerException) : base(message, innerException) { }
    }
}
