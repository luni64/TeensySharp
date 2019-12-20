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
        
        public Guid DeviceClassGuid { get; internal set; }
        
        public Guid ContainerGuid { get; internal set; }
        public IUsbDevice Parent { get; internal set; }
        public List<IUsbDevice> children { get; } = new List<IUsbDevice>();
        public int vid { get; internal set; }
        public int pid { get; internal set; }

        internal string ParentStr { get; set; }

        public UsbDevice(Guid deviceClassGuid)
        {
            this.DeviceClassGuid = deviceClassGuid;
        }

        public override string ToString()
        {
            return $"{Description} ({vid:X4}/{pid:X4}) #{Serialnumber}";
        }
    }

    internal class UsbSerial : UsbDevice, IUsbSerial
    {
        public UsbSerial() : base(DeviceClassGuids.Ports)
        {
            Port = null;
        }
        public string Port { get; set; }

        public override string ToString()
        {
            return $"{Description} {Port} ({vid:X4}/{pid:X4}) #{Serialnumber}";          
        }
    }

    public class UsbHid : UsbDevice
    {
        public UsbHid() : base(DeviceClassGuids.HidClass)
        {
          
        }        
    }

}
