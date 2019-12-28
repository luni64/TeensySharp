using System;
using System.Collections.Generic;
using static lunOptics.LibUsbTree.Implementation.NativeMethods;

namespace lunOptics.LibUsbTree.Implementation
{
    internal class InfoSet : IDisposable
    {
        public InfoSet(String Enumerator)
        {
            deviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, Enumerator, IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

            deviceInfoData = new SP_DEVINFO_DATA()
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SP_DEVINFO_DATA))
            };
        }

        public IEnumerable<DeviceInfo> getDeviceInstances()
        {
            for (uint index = 0; SetupDiEnumDeviceInfo(deviceInfoSet, index, ref deviceInfoData); index++)
            {
                yield return new DeviceInfo(deviceInfoSet, deviceInfoData);
            }
        }

        public void Dispose()
        {            
            SetupDiDestroyDeviceInfoList(deviceInfoSet);
        }

        private SP_DEVINFO_DATA deviceInfoData;
        private readonly IntPtr deviceInfoSet;       
    }
}

