
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using static lunOptics.UsbTree.Implementation.NativeUsb;
using static lunOptics.UsbTree.Implementation.UsbWrappers;



namespace lunOptics.UsbTree.Implementation
{
    public static class UsbTree
    {

        public static void setCallback(Func<UsbDevice, UsbDevice> callback)
        {
            cb = callback;
        }

        static Func<UsbDevice, UsbDevice> cb = null;

        public static List<IUsbDevice> getDevices()
        {
            var devices = new List<IUsbDevice>();

            using (var ctx = new InfoSetContext("USB"))
            {
                foreach (var inst in ctx.getDeviceInstances())
                {
                    var name = inst.getProperty<string>(DevPropKey.DeviceDesc);
                    Debug.WriteLine(name);
                    inst.Dispose();
                }
            }



            UsbWrappers.deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, "USB", IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));
            for (uint index = 0; SetupDiEnumDeviceInfo(UsbWrappers.deviceInfoSet, index, ref UsbWrappers.deviceInfoData); index++)
            {
                UsbDevice usbdev = new UsbDevice(UsbWrappers.deviceInfoData.ClassGuid);
                usbdev.DeviceInstanceId = getDeviceProperty<string>(DevPropKey.DeviceInstanceId).ToUpper();


                try
                {
                    var devInstId_parts = usbdev.DeviceInstanceId.Split('\\');
                    var device_parts = devInstId_parts[1].Split('&');

                    usbdev.Serialnumber = devInstId_parts[2];          // should always be present
                    usbdev.IsUsbInterface = device_parts.Length == 3;  // interfaces have &MI_NN added to the vid/pid part
                    if (device_parts.Length >= 2)
                    {
                        usbdev.vid = Convert.ToInt32(device_parts[0].Substring(4, 4), 16);
                        usbdev.pid = Convert.ToInt32(device_parts[1].Substring(4, 4), 16);
                    }
                }
                catch (Exception innerException)
                {
                    throw new UsbTreeException($"Error parsing DeviceInstanceId {usbdev.DeviceInstanceId}", innerException);
                }


                usbdev = cb?.Invoke(usbdev);

                usbdev.DeviceClass = getDeviceProperty<String>(DevPropKey.DeviceClass);
                usbdev.ParentStr = getDeviceProperty<String>(DevPropKey.Parent).ToUpper();
                usbdev.Description = getDeviceProperty<String>(DevPropKey.BusReportedDeviceDesc) ?? getDeviceProperty<String>(DevPropKey.DeviceDesc);

                if (usbdev == null)
                {

                    switch (deviceInfoData.ClassGuid)
                    {
                        case var g when (g == DeviceClassGuids.Ports):
                            OpenDeviceRegistryKey();
                            usbdev = new UsbSerial()
                            {
                                Port = getDeviceRegPropertry<string>("PortName"),
                            };
                            break;

                        case var g when (g == DeviceClassGuids.HidClass):
                            usbdev = new UsbHid();
                            break;

                        default:
                            usbdev = new UsbDevice(deviceInfoData.ClassGuid);
                            break;

                    }

                }



                devices.Add(usbdev);
            }
            SetupDiDestroyDeviceInfoList(deviceInfoSet);

            // link parent and children
            foreach (UsbDevice device in devices)
            {
                device.Parent = devices.FirstOrDefault(d => d.DeviceInstanceId == device.ParentStr);
                device.Parent?.children.Add(device);
            }
            return devices;
        }

        public static void print(IUsbDevice d, int level = 0)
        {
            Debug.Write(new String(' ', level * 2));
            Debug.WriteLine(d.DeviceClass + " " + d.ToString());
            foreach (var c in d.children)//.Where(c=>c.IsUsbInterface == false))
            {
                print(c, level + 1);
            }
        }

        //private static SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA() { cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)) };
        //private static IntPtr deviceInfoSet;

        private const int bufSize = 1024;
        private static readonly IntPtr buffer = Marshal.AllocHGlobal(bufSize);


    }
}
