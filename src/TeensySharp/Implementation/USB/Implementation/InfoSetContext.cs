using lunOptics.UsbTree.Implementation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static lunOptics.UsbTree.Implementation.NativeUsb;
//using static lunOptics.UsbTree.Implementation.UsbWrappers;

namespace lunOptics.UsbTree.Implementation
{

    internal class DeviceInstance : IDisposable
    {
        public DeviceInstance(IntPtr deviceInfoSet, SP_DEVINFO_DATA deviceInfoData) 
        {
            bufSize = 1024;
            buffer = Marshal.AllocHGlobal(bufSize);
            this.deviceInfoSet = deviceInfoSet;
            this.deviceInfoData = deviceInfoData;            
        }
        
        public T getProperty<T>(DevPropKey key)
        {
            var devPropKey = DevPropK[key];

            if (true == SetupDiGetDevicePropertyW(deviceInfoSet, ref deviceInfoData, ref devPropKey, out ulong propertyType, buffer, bufSize, out int requiredSize, 0))
            {
                

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)Marshal.PtrToStringAuto(buffer);
                }
                else if (typeof(T) == typeof(Guid))
                {
                    return (T)(object)Marshal.PtrToStructure<T>(buffer);
                }
            }
            else
            {
                int err = Marshal.GetLastWin32Error();
            }
            return default;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(buffer);
        }

        private static DevPropKeys DevPropK = new DevPropKeys();
        private IntPtr buffer;
        private int bufSize;

        private IntPtr deviceInfoSet;
        private SP_DEVINFO_DATA deviceInfoData;
    }

    internal class InfoSetContext : IDisposable
    {
        public InfoSetContext(String Enumerator)
        {
            deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, Enumerator, IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

            deviceInfoData = new SP_DEVINFO_DATA()
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SP_DEVINFO_DATA))
            };


        }

        public IEnumerable<DeviceInstance> getDeviceInstances()
        {
            for (uint index = 0; SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData); index++)
            {
                yield return new DeviceInstance(deviceInfoSet, deviceInfoData);
            }
        }



        public void Dispose()
        {
            //Marshal.FreeHGlobal(buffer);
        }


        private static SP_DEVINFO_DATA deviceInfoData;
        private static IntPtr deviceInfoSet;

        //private const int bufSize = 2048;
        //private static readonly IntPtr buffer = Marshal.AllocHGlobal(bufSize);

        //private static DevPropKeys DevPropK = new DevPropKeys();
    }
}

