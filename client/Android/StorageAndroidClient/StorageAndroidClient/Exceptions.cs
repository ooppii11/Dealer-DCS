using System;

namespace StorageAndroidClient
{
    [Serializable]
    public class StoragePermissionsDenied : Exception
    {
        public StoragePermissionsDenied() { }
        public StoragePermissionsDenied(string message) : base(message) { }
        public StoragePermissionsDenied(string message, Exception inner) : base(message, inner) { }
        protected StoragePermissionsDenied(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}