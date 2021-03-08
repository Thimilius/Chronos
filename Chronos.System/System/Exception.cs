namespace System {
    public class Exception {
#pragma warning disable CS0169
#pragma warning disable CS0649
        private string m_Message;
        public string Message => m_Message;

        private Exception m_InnerException;
        public Exception InnerException => m_InnerException;
#pragma warning restore CS0649
#pragma warning restore CS0169

        public Exception() {

        }

        public Exception(string message) {
            m_Message = message;
        }

        public Exception(string message, Exception innerException) : this(message) {
            m_InnerException = innerException;
        }

        public override string ToString() {
            return Message;
        }
    }
}
