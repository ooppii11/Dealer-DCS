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
    public class AuthenticationException : Exception
    {
        public AuthenticationException() { }
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception inner) : base(message, inner) { }
        protected AuthenticationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FileErrorsException : Exception
    {
        public FileErrorsException() { }
        public FileErrorsException(string message) : base(message) { }
        public FileErrorsException(string message, Exception inner) : base(message, inner) { }
        protected FileErrorsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class DBErrorException : Exception
    {
        public DBErrorException() { }
        public DBErrorException(string message) : base(message) { }
        public DBErrorException(string message, Exception inner) : base(message, inner) { }
        protected DBErrorException(
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

    [Serializable]
    public class FileDoesNotExistException : FileErrorsException
    {
        public FileDoesNotExistException() { }
        public FileDoesNotExistException(string message) : base(message) { }
        public FileDoesNotExistException(string message, Exception inner) : base(message, inner) { }
        protected FileDoesNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class FileAlreadyExistException : FileErrorsException
    {
        public FileAlreadyExistException() { }
        public FileAlreadyExistException(string message) : base(message) { }
        public FileAlreadyExistException(string message, Exception inner) : base(message, inner) { }
        protected FileAlreadyExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IncorrectSessionIdException : AuthenticationException
    {
        public IncorrectSessionIdException() { }
        public IncorrectSessionIdException(string message) : base(message) { }
        public IncorrectSessionIdException(string message, Exception inner) : base(message, inner) { }
        protected IncorrectSessionIdException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UserAlreadyLoggedInException : AuthenticationException
    {
        public UserAlreadyLoggedInException() { }
        public UserAlreadyLoggedInException(string message) : base(message) { }
        public UserAlreadyLoggedInException(string message, Exception inner) : base(message, inner) { }
        protected UserAlreadyLoggedInException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class UserDoesNotExistException : AuthenticationException
    {
        public UserDoesNotExistException() { }
        public UserDoesNotExistException(string message) : base(message) { }
        public UserDoesNotExistException(string message, Exception inner) : base(message, inner) { }
        protected UserDoesNotExistException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
