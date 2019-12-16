//using MoreLinq.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using TeensySharpLib.Implementation.usb;
using static TeensySharpLib.Implementation.usb.NativeUsb;

namespace lunOptics.UsbTree.Implementation
{
    public static class UsbTree
    {
        public static List<IUsbDevice> getDevices()
        {
            var devices = new List<IUsbDevice>();
            var devInfoSet = SetupDiGetClassDevs(IntPtr.Zero, "USB", IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

            var devInfoData = new SP_DEVINFO_DATA()
            {
                cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)),
                ClassGuid = Guid.Empty,
                DevInst = 0,
                Reserved = IntPtr.Zero
            };
                        
            for (uint devIndex = 0;  SetupDiEnumDeviceInfo(devInfoSet, devIndex, ref devInfoData); devIndex++)
            {
                var usbdev = new UsbDevice()
                {
                    DeviceInstanceId = getDevProp<String>(DevPropKeys.DeviceInstanceId, devInfoSet, devInfoData).ToUpper(),
                    ContainerGuid = getDevProp<Guid>(DevPropKeys.ContainerID, devInfoSet, devInfoData),
                    Description = getDevProp<String>(DevPropKeys.BusReportedDeviceDesc, devInfoSet, devInfoData),                   
                    DeviceClass = getDevProp<String>(DevPropKeys.DeviceClass, devInfoSet, devInfoData),
                    ParentStr = getDevProp<String>(DevPropKeys.Parent, devInfoSet, devInfoData).ToUpper(),
                };

                if (String.IsNullOrEmpty(usbdev.Description))
                    usbdev.Description = getDevProp<String>(DevPropKeys.DeviceDesc, devInfoSet, devInfoData);                 

                devices.Add(usbdev);
            }
            SetupDiDestroyDeviceInfoList(devInfoSet);

            


            // link parent and children
            foreach (UsbDevice device in devices)
            {
                device.Parent = devices.FirstOrDefault(d => d.DeviceInstanceId == device.ParentStr);               
                if (device.Parent != null)
                {
                    device.Parent.children.Add(device);
                }
            }

            return devices;
        }

        public static void print(IUsbDevice d, int level=0)
        {
            Debug.Write(new String(' ', level * 2));
            Debug.WriteLine(d.ToString());
            foreach (var c in d.children)
            {
                print(c, level + 1);
            }
        }

        const int bufSize = 1024;
        static readonly IntPtr buffer = Marshal.AllocHGlobal(bufSize);

        internal static T getDevProp<T>(DEVPROPKEY key, IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData)
        {
            if (true == SetupDiGetDevicePropertyW(deviceInfoSet, ref deviceInfoData, ref key, out ulong propertyType, buffer, bufSize, out int requiredSize, 0))
            {
                if (typeof(String).IsEquivalentTo(typeof(T)))
                {
                    return (T)(object)Marshal.PtrToStringAuto(buffer);
                }
                else if (typeof(Guid).IsEquivalentTo(typeof(T)))
                {
                    return (T)(object)Marshal.PtrToStructure<Guid>(buffer);
                }
            }
            return default;
        }
    }
}
