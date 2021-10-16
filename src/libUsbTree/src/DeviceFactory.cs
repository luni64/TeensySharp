using lunOptics.libUsbTree.Implementation;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace lunOptics.libUsbTree 
{
    public class DeviceFactory
    {
        // override in a derived class
        public virtual IUsbDevice newDevice(InfoNode deviceInfo)
        {
            return new UsbDevice(deviceInfo);
        }

        internal IUsbDevice MakeOrUpdate(InfoNode deviceInfo)
        {
            var cached = repository.FirstOrDefault(d => d.isEqual(deviceInfo));
            if (cached == null)
            {
                var device = newDevice(deviceInfo);
                repository.Add((UsbDevice)device);
               // var idx = repository.IndexOf((UsbDevice)device);
               // Trace.WriteLine($"ADD:  #{idx} {device.Description} - {device.DeviceInstanceID}");
                return device;
            }
            else
            {
                cached.update(deviceInfo);
                //var idx = repository.IndexOf(cached);
                //if (deviceInfo.vid == 0x16c0)  Trace.WriteLine($"UPD: #{idx} {cached.Description} - {cached.DeviceInstanceID}");
                return cached;
            }
        }

        private readonly List<UsbDevice> repository = new List<UsbDevice>();
    };
}

