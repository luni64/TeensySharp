using BetterWin32Errors;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static lunOptics.LibUsbTree.Implementation.NativeMethods;

namespace lunOptics.LibUsbTree.Implementation
{
    /// <summary>
    /// Context for querying device infos. Dispose after using
    /// </summary>
    internal class UsbInfoSet : IDisposable
    {
        public UsbInfoSet()
        {
            hDeviceInfoSet = SetupDiGetClassDevs(IntPtr.Zero, "USB", IntPtr.Zero, (int)(DiGetClassFlags.DIGCF_ALLCLASSES | DiGetClassFlags.DIGCF_PRESENT));

            deviceInfoData = new SP_DEVINFO_DATA()
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(SP_DEVINFO_DATA))
            };
        }
               
        public HashSet<string> getDeviceIDs()
        {
            HashSet<string> newIDs = new HashSet<string>();

            //query the current device list
            IntPtr buffer = Marshal.AllocHGlobal(1024);
            for (uint index = 0; SetupDiEnumDeviceInfo(hDeviceInfoSet, index, ref deviceInfoData); index++)
            {
                if (!SetupDiGetDevicePropertyW(hDeviceInfoSet, ref deviceInfoData, ref DevicePropertyKeys.DeviceInstanceID, out _, buffer, 1024, out _, 0))
                {
                    throw new Win32Exception("getConnectedDevcieIDs");
                }
                newIDs.Add(Marshal.PtrToStringAuto(buffer));
            }
            Marshal.FreeHGlobal(buffer);

            return newIDs;          
        }
               
        public IEnumerable<UsbDeviceInfo> getDeviceInfos()
        {
            for (uint index = 0; SetupDiEnumDeviceInfo(hDeviceInfoSet, index, ref deviceInfoData); index++)
            {
                yield return new UsbDeviceInfo(hDeviceInfoSet, deviceInfoData);
            }
        }
                
        public void Dispose()
        {
            SetupDiDestroyDeviceInfoList(hDeviceInfoSet);
        }
        
        private SP_DEVINFO_DATA deviceInfoData;
        private readonly IntPtr hDeviceInfoSet;
    }
}

