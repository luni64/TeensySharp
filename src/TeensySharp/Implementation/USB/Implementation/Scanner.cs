
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using static lunOptics.UsbTree.Implementation.NativeUsb;



namespace lunOptics.UsbTree.Implementation
{
    public static class UsbTree
    {
        public static List<IUsbDevice> getDevices()
        {
            UsbDevice usbdev;
            var devices = new List<IUsbDevice>();

            deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, "USB", IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

            for (uint index = 0; SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData); index++)
            {
                var DeviceClassGuid = getDeviceProperty<Guid>(DevPropKeys.DeviceClassGuid);

                if (DeviceClassGuid == DeviceClassGuids.Ports)        // SERIAL ----------------------   
                {
                    usbdev = new UsbSerial();
                                       
                    IntPtr h= SetupDiOpenDevRegKey(deviceInfoSet, ref deviceInfoData, (int)DICS_FLAG.DICS_FLAG_GLOBAL, 0, (int) DIREG.DIREG_DEV, 1);

                    int sz = bufSize;
                    IntPtr ip = IntPtr.Zero;
                    int kind = 1;

                    var res = RegQueryValueEx(h, "PortName", 0, ref kind, buffer, ref sz);
                    var err = Marshal.GetLastWin32Error();
                    ((UsbSerial)usbdev).Port = Marshal.PtrToStringAnsi(buffer);

                }
                else if (DeviceClassGuid == DeviceClassGuids.HidClass) // HID -------------------------
                {
                    usbdev = new UsbHid();
                }
                else usbdev = new UsbDevice(DeviceClassGuid);


                usbdev.DeviceInstanceId = getDeviceProperty<String>(DevPropKeys.DeviceInstanceId).ToUpper();
                usbdev.DeviceClass = getDeviceProperty<String>(DevPropKeys.DeviceClass);
                usbdev.ParentStr = getDeviceProperty<String>(DevPropKeys.Parent).ToUpper();

                usbdev.Description = getDeviceProperty<String>(DevPropKeys.BusReportedDeviceDesc);
                if (String.IsNullOrEmpty(usbdev.Description))
                    usbdev.Description = getDeviceProperty<String>(DevPropKeys.DeviceDesc);

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
            Debug.WriteLine(d.ToString());
            foreach (var c in d.children)
            {
                print(c, level + 1);
            }
        }

        private static SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA() { cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)) };
        private static IntPtr deviceInfoSet;

        private const int bufSize = 1024;
        private static readonly IntPtr buffer = Marshal.AllocHGlobal(bufSize);

        private static T getDeviceProperty<T>(DEVPROPKEY key)
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
