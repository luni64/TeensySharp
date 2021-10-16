using System;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA1712 // Do not prefix enum values with type name
#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA1051 // Do not declare visible instance fields

namespace lunOptics.libUsbTree.Implementation
{
    internal static class NativeMethods
    {
        //========================================================================================================================================
        // Setupapi.h
        // https://docs.microsoft.com/en-us/windows/win32/api/setupapi/
        //========================================================================================================================================

        #region pinvoke -------------------------------------------
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetupDiGetClassDevs(
            IntPtr ClassGuid,
            string Enumerator,
            IntPtr hwndParent,
            int Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid classGuid,
            string enumerator,
            int hwndParent,
            int Flags
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInfo(
            IntPtr DeviceInfoSet,
            uint MemberIndex,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDevicePropertyW(
            IntPtr deviceInfoSet,
        ref SP_DEVINFO_DATA DeviceInfoData,
        ref DEVPROPKEY propertyKey,
            out UInt64 propertyType,
            IntPtr propertyBuffer,
            Int32 propertyBufferSize,
        out int requiredSize,
            UInt32 flags
        );

        [DllImport("setupapi.dll")]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr deviceInfoSet,
        ref SP_DEVINFO_DATA deviceInfoData,
        ref Guid interfaceClassGuid,
            int memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );


        #endregion

        #region Structs and Flags -------------------------------------------

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

        [StructLayout(LayoutKind.Sequential)]
        internal struct SP_DEVICE_INTERFACE_DATA
        {
            internal int cbSize;
            internal System.Guid InterfaceClassGuid;
            internal int Flags;
            internal IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            internal int Size;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            internal string DevicePath;
        }

        /// <summary>
        /// KeyType values for SetupDiCreateDevRegKey, SetupDiOpenDevRegKey, and SetupDiDeleteDevRegKey.
        /// </summary>
        internal enum DIREG : int
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
        internal enum DICS_FLAG : int
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
        #endregion


        //========================================================================================================================================
        // CfgMgr32.h
        // https://docs.microsoft.com/en-us/windows/win32/devinst/cfgmgr32-h
        //========================================================================================================================================

        #region pinvoke -----------------------------------------------------

        //CM_Locate_DevNode
        [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern CR CM_Locate_DevNode(
        out int pdnDevInst,
            string pDeviceID,
            CM_LOCATE_DEVNODE ulFlags
        );

        //CM_Get_Device_ID_List_Size
        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern CR CM_Get_Device_ID_List_Size(
        out int idListlen,
            string filter,
            CM_GETIDLIST ulFlags
         );

        // CM_Get_Device_ID_List
        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern CR CM_Get_Device_ID_List(
            string filter,
            byte[] buffer,
            int bffrLen,
            CM_GETIDLIST ulFlags
        );

        //CM_Get_Device_Interface_List
        [DllImport("CfgMgr32.dll", CharSet = CharSet.Unicode)]
        public static extern CR CM_Get_Device_Interface_List(
        ref Guid interfaceClassGuid,
            string deviceID,
            byte[] buffer,
            int bufferLength,
            CM_GET_DEVICE_INTERFACE_LIST flags
        );

        //CM_Get_Parent
        [DllImport("CfgMgr32.dll", SetLastError = true)]
        public static extern CR CM_Get_Parent(
            out int pdnDevInst,
                int dnDevInst,
                uint ulFlags = 0 // must always be '0'
        );

        //CM_Get_Child
        [DllImport("CfgMgr32.dll", SetLastError = true)]
        public static extern CR CM_Get_Child(
            out int pdnDevInst,
                int dnDevInst,
                uint ulFlags = 0 // must always be '0'
        );

        //CM_Get_Sibling
        [DllImport("CfgMgr32.dll", SetLastError = true)]
        public static extern CR CM_Get_Sibling(
            out int pdnDevInst,
                int dnDevInst,
                uint ulFlags = 0 // must always be '0'
        );

        //CM_Get_Device_ID
        [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern CR CM_Get_Device_ID(
            int dnDevInst,
            StringBuilder Buffer,
            int BufferLen,
            int ulFlags = 0 // must be 0
        );

        //CM_Get_DevNode_PropertyW
        [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern CR CM_Get_DevNode_PropertyW(
            int dnDevInst,
        ref DEVPROPKEY PropertyKey,
        out UInt64 PropertyType,
            byte[] PropertyBuffer,
        ref int PropertyBufferSize,
            uint ulFlags = 0
        );

        //CM_Get_DevNode_Status
        [DllImport("CfgMgr32.dll", SetLastError = true)]
        public static extern CR CM_Get_DevNode_Status(
            out uint pulStatus,
            out uint pulProblemNumber,
                uint dnDevInst,
                uint ulFlags // must always be '0'
        );

        //CM_Reenumerate_DevNode
        [DllImport("CfgMgr32.dll", SetLastError = true)]
        public static extern CR CM_Reenumerate_DevNode(
            int pdnDevInst,
            CM_REENUMERATE ulFlags
        );



        #endregion

        #region Structs and Flags -------------------------------------------
        public enum CR
        {
            SUCCESS = 0x00000000,
            DEFAULT = 0x00000001,
            OUT_OF_MEMORY = 0x00000002,
            INVALID_POINTER = 0x00000003,
            INVALID_FLAG = 0x00000004,
            INVALID_DEVNODE = 0x00000005,
            INVALID_DEVINST = CR.INVALID_DEVNODE,
            INVALID_RES_DES = 0x00000006,
            INVALID_LOG_CONF = 0x00000007,
            INVALID_ARBITRATOR = 0x00000008,
            INVALID_NODELIST = 0x00000009,
            DEVNODE_HAS_REQS = 0x0000000A,
            DEVINST_HAS_REQS = CR.DEVNODE_HAS_REQS,
            INVALID_RESOURCEID = 0x0000000B,
            DLVXD_NOT_FOUND = 0x0000000C,
            NO_SUCH_DEVNODE = 0x0000000D,
            NO_SUCH_DEVINST = CR.NO_SUCH_DEVNODE,
            NO_MORE_LOG_CONF = 0x0000000E,
            NO_MORE_RES_DES = 0x0000000F,
            ALREADY_SUCH_DEVNODE = 0x00000010,
            ALREADY_SUCH_DEVINST = CR.ALREADY_SUCH_DEVNODE,
            INVALID_RANGE_LIST = 0x00000011,
            INVALID_RANGE = 0x00000012,
            FAILURE = 0x00000013,
            NO_SUCH_LOGICAL_DEV = 0x00000014,
            CREATE_BLOCKED = 0x00000015,
            NOT_SYSTEM_VM = 0x00000016,
            REMOVE_VETOED = 0x00000017,
            APM_VETOED = 0x00000018,
            INVALID_LOAD_TYPE = 0x00000019,
            BUFFER_SMALL = 0x0000001A,
            NO_ARBITRATOR = 0x0000001B,
            NO_REGISTRY_HANDLE = 0x0000001C,
            REGISTRY_ERROR = 0x0000001D,
            INVALID_DEVICE_ID = 0x0000001E,
            INVALID_DATA = 0x0000001F,
            INVALID_API = 0x00000020,
            DEVLOADER_NOT_READY = 0x00000021,
            NEED_RESTART = 0x00000022,
            NO_MORE_HW_PROFILES = 0x00000023,
            DEVICE_NOT_THERE = 0x00000024,
            NO_SUCH_VALUE = 0x00000025,
            WRONG_TYPE = 0x00000026,
            INVALID_PRIORITY = 0x00000027,
            NOT_DISABLEABLE = 0x00000028,
            FREE_RESOURCES = 0x00000029,
            QUERY_VETOED = 0x0000002A,
            CANT_SHARE_IRQ = 0x0000002B,
            NO_DEPENDENT = 0x0000002C,
            SAME_RESOURCES = 0x0000002D,
            NO_SUCH_REGISTRY_KEY = 0x0000002E,
            INVALID_MACHINENAME = 0x0000002F,
            REMOTE_COMM_FAILURE = 0x00000030,
            MACHINE_UNAVAILABLE = 0x00000031,
            NO_CM_SERVICES = 0x00000032,
            ACCESS_DENIED = 0x00000033,
            CALL_NOT_IMPLEMENTED = 0x00000034,
            INVALID_PROPERTY = 0x00000035,
            DEVICE_INTERFACE_ACTIVE = 0x00000036,
            NO_SUCH_DEVICE_INTERFACE = 0x00000037,
            INVALID_REFERENCE_STRING = 0x00000038,
            INVALID_CONFLICT_LIST = 0x00000039,
            INVALID_INDEX = 0x0000003A,
            INVALID_STRUCTURE_SIZE = 0x0000003B,
        }

        [Flags]
        public enum CM_LOCATE_DEVNODE : uint
        {
            NORMAL = 0x00000000,
            PHANTOM = 0x00000001,
            CANCELREMOVE = 0x00000002,
            NOVALIDATION = 0x00000004,
            BITS = 0x00000007
        }

        [Flags]
        public enum CM_REENUMERATE : uint
        {
            NORMAL = 0x00000000,
            SYNCHRONOUS = 0x00000001,
            RETRY_INSTALLATION = 0x00000002,
            ASYNCHRONOUS = 0x00000004,
            BITS = 0x00000007
        }

        [Flags]
        public enum CM_GETIDLIST : uint
        {
            FILTER_NONE = 0x00000000,
            FILTER_ENUMERATOR = 0x00000001,
            FILTER_SERVICE = 0x00000002,
            FILTER_EJECTRELATIONS = 0x00000004,
            FILTER_REMOVALRELATIONS = 0x00000008,
            FILTER_POWERRELATIONS = 0x00000010,
            FILTER_BUSRELATIONS = 0x00000020,
            DONOTGENERATE = 0x10000040,
            FILTER_TRANSPORTRELATIONS = 0x00000080,
            FILTER_PRESENT = 0x00000100,
            FILTER_CLASS = 0x00000200,
            FILTER_BITS = 0x100003FF,
        }

        [Flags]
        public enum CM_GET_DEVICE_INTERFACE_LIST : uint
        {
            PRESENT = 0,
            ALL_DEVICES = 1,
        }

        #endregion
    }
}

#pragma warning restore CA1712 // Do not prefix enum values with type name
#pragma warning restore CA1815 // Override equals and operator equals on value types
#pragma warning restore CA1051 // Do not declare visible instance fields
#pragma warning restore CA1707 // Identifiers should not contain underscores