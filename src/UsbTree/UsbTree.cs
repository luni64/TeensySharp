using lunOptics.LibUsbTree.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace lunOptics.LibUsbTree
{
    public class UsbTree
    {
        public List<UsbDevice> getDevices()
        {
            var devices = new List<UsbDevice>();

            using (var infoSet = new InfoSet("USB"))
            {
                foreach (var instance in infoSet.getDeviceInstances())
                {
                    devices.Add(MakeDevice(instance)); // MakeDevice is virtual to allow specialiced subclasses adding their devices
                    instance.Dispose();
                }

                foreach (UsbDevice device in devices)
                {
                    device.Parent = devices.FirstOrDefault(d => d.DeviceInstanceId == device.ParentStr);
                    device.Parent?.children.Add(device);
                }
            }
            return devices;
        }

        public virtual UsbDevice MakeDevice(DeviceInfo inst)
        {
            return new UsbDevice(inst); // generic device, override to generate more specialized devices           
        }


        public static void print(UsbDevice device, int level = 0)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));

            Debug.Write(new String(' ', level * 2));
            Debug.WriteLine(device.ToString());
            foreach (var c in device.children)//.Where(c=>c.IsUsbInterface == false))
            {
                print(c, level + 1);
            }
        }
    }
}
