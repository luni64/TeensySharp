using System;
using System.Collections.Generic;

namespace lunOptics.UsbTree.Implementation
{
    public interface IUsbDevice
    {
        String DeviceInstanceId { get;  }
        string Serialnumber { get;  }
        String Description { get;  }
        String DeviceClass { get;  }        
        IUsbDevice Parent { get;  }
        List<IUsbDevice> children { get; }        
        bool IsUsbInterface { get;  }
    }
}