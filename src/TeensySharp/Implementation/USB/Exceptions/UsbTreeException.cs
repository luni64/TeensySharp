using System;
using System.Collections.Generic;
using System.Text;

namespace lunOptics.UsbTree
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
    }
}
