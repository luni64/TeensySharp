using System;
using System.Collections.Generic;

namespace lunOptics.UsbTree.Implementation
{
    public class UsbDevice : IUsbDevice
    {
        public bool IsUsbInterface { get; internal set; }
        public string DeviceInstanceId { get; internal set; }
        public string Description { get; internal set; }
        public string Serialnumber { get; internal set; }
        public string DeviceClass { get; internal set; }
        public Guid ContainerGuid { get; internal set; }
        public IUsbDevice Parent { get; internal set; }
        public List<IUsbDevice> children { get; } = new List<IUsbDevice>();
        
        internal string ParentStr { get;  set; }
        
        public override string ToString()
        {
            return $"{Description} ({DeviceInstanceId})";
        }

    }
}
