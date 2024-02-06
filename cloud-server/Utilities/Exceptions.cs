namespace cloud_server.Utilities
{
    [Serializable]
    public class NoLeaderException : Exception
    {
        public NoLeaderException() { }
        public NoLeaderException(string message) : base(message) { }
        public NoLeaderException(string message, Exception inner) : base(message, inner) { }
        protected NoLeaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class NoEntryException : NoLeaderException
    {
        public NoEntryException() { }
        public NoEntryException(string message) : base(message) { }
        public NoEntryException(string message, Exception inner) : base(message, inner) { }
        protected NoEntryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class EmptyEntryException : NoLeaderException
    {
        public EmptyEntryException() { }
        public EmptyEntryException(string message) : base(message) { }
        public EmptyEntryException(string message, Exception inner) : base(message, inner) { }
        protected EmptyEntryException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
