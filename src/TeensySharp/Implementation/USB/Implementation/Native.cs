using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace TeensySharpLib.Implementation.usb
{
    internal static class NativeUsb
    {
        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("setupapi.dll", SetLastError = true)]
        internal static extern bool SetupDiGetDevicePropertyW(
               IntPtr deviceInfoSet,
               ref SP_DEVINFO_DATA DeviceInfoData,
               ref DEVPROPKEY propertyKey,
               out UInt64 propertyType, 
               IntPtr propertyBuffer, 
               Int32 propertyBufferSize,
               out int requiredSize, 
               UInt32 flags);


        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, int ulFlags = 0);
    }


    [Flags]
    internal enum DiGetClassFlags : uint
    {
        DIGCF_DEFAULT = 0x00000001,  // only valid with DIGCF_DEVICEINTERFACE
        DIGCF_PRESENT = 0x00000002,
        DIGCF_ALLCLASSES = 0x00000004,
        DIGCF_PROFILE = 0x00000008,
        DIGCF_DEVICEINTERFACE = 0x00000010,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct SP_DEVINFO_DATA
    {
        public UInt32 cbSize;
        public Guid ClassGuid;
        public UInt32 DevInst;
        public IntPtr Reserved;
    }

    // Device Property
    [StructLayout(LayoutKind.Sequential)]
    internal struct DEVPROPKEY
    {
        public Guid fmtid;
        public UInt32 pid;
    }


}
