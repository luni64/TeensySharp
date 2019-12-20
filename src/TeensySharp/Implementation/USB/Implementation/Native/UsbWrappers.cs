using lunOptics.UsbTree.Implementation;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static lunOptics.UsbTree.Implementation.NativeUsb;


namespace lunOptics.UsbTree.Implementation
{
    internal static class UsbWrappers
    {
        public static T getDeviceProperty<T>(DevPropKey key)
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

        static internal void OpenDeviceRegistryKey()
        {
            regHandle = SetupDiOpenDevRegKey(deviceInfoSet, ref deviceInfoData, (int)DICS_FLAG.DICS_FLAG_GLOBAL, 0, (int)DIREG.DIREG_DEV, 1);
        }

        static IntPtr regHandle;       

        internal static T getDeviceRegPropertry<T>(string key)
        {
            int kind = 1;
            int sz = bufSize;
            var sb = new StringBuilder(bufSize);
            var ip = IntPtr.Zero;

            int errr = 0; 

            if ((errr = RegQueryValueEx(regHandle, key, 0, ref kind, sb, ref sz)) == 0)
            {
                if (typeof(T) == typeof(string))
                {
                    return (T)(object) sb.ToString();
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

        private static DevPropKeys DevPropK = new DevPropKeys();

        internal static SP_DEVINFO_DATA deviceInfoData = new SP_DEVINFO_DATA() { cbSize = (uint)Marshal.SizeOf(typeof(SP_DEVINFO_DATA)) };
        internal static IntPtr deviceInfoSet;

        private const int bufSize = 2048;
        private static readonly IntPtr buffer = Marshal.AllocHGlobal(bufSize);

    }
}
