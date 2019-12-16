using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace lunOptics.UsbTree.Implementation
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
        internal static extern int CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, int BufferLen, int ulFlags = 0);


        [DllImport("Setupapi", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr SetupDiOpenDevRegKey(IntPtr hDeviceInfoSet, ref SP_DEVINFO_DATA deviceInfoData, int scope, int hwProfile, int parameterRegistryValueKind, int samDesired);

        [DllImport("advapi32.dll", SetLastError = true)]
        internal static extern uint RegQueryValueEx(IntPtr hKey, string lpValueName, int lpReserved, ref int lpType, IntPtr lpData, ref int lpcbData);

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
    public struct DEVPROPKEY
    {
        public Guid fmtid;
        public UInt32 pid;
    }

    /// <summary>
    /// KeyType values for SetupDiCreateDevRegKey, SetupDiOpenDevRegKey, and SetupDiDeleteDevRegKey.
    /// </summary>
    public enum DIREG : int
    {
        /// <summary>
        /// Open/Create/Delete device key
        /// </summary>
        DIREG_DEV = 0x00000001,

        /// <summary>
        /// Open/Create/Delete driver key
        /// </summary>
        DIREG_DRV = 0x00000002,

        /// <summary>
        /// Delete both driver and Device key
        /// </summary>
        DIREG_BOTH = 0x00000004,
    }

    /// <summary>
    /// Values specifying the scope of a device property change
    /// </summary>
    public enum DICS_FLAG : int
    {
        /// <summary>
        /// make change in all hardware profiles
        /// </summary>
        DICS_FLAG_GLOBAL = 0x00000001,

        /// <summary>
        /// make change in specified profile only
        /// </summary>
        DICS_FLAG_CONFIGSPECIFIC = 0x00000002,

        /// <summary>
        /// 1 or more hardware profile-specific
        /// </summary>
        DICS_FLAG_CONFIGGENERAL = 0x00000004,
    }

}
