namespace System {
    public class NotFiniteNumberException : ArithmeticException {
        public NotFiniteNumberException() { }

        public NotFiniteNumberException(string message) : base(message) { }

        public NotFiniteNumberException(string message, Exception innerException) : base(message, innerException) { }
    }
}
