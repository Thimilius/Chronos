namespace System {
    public class NullReferenceException : SystemException {
        public NullReferenceException() { }

        public NullReferenceException(string message) : base(message) { }

        public NullReferenceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
