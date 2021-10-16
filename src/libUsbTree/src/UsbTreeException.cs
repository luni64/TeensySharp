using System;
using System.Collections.Generic;
using System.Text;

namespace lunOptics.libUsbTree
{
    [Serializable]
    public class UsbTreeException : Exception
    {
        public UsbTreeException()
        { }

        public UsbTreeException(string message)
            : base(message)
        { }

        public UsbTreeException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected UsbTreeException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
            throw new NotImplementedException();
        }
    }
}
