using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using static lunOptics.libTeensySharp.Native.NativeMethods;

namespace lunOptics.libTeensySharp.Native
{
    public static class NativeWrapper
    {
        public static string[] cmGetDeviceInterfaces(string deviceInstanceId, Guid interfaceGuid, int bufSize = 1024 * 10)
        {

            var buffer = new byte[bufSize];
            CR result = CM_Get_Device_Interface_List(ref interfaceGuid, deviceInstanceId, buffer, bufSize, CM_GET_DEVICE_INTERFACE_LIST.PRESENT);
            if (result != CR.SUCCESS) throw new ApplicationException(nameof(cmGetDeviceInterfaces));

            return Encoding.Unicode.GetString(buffer, 0, bufSize).Trim('\0').Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static SafeFileHandle getFileHandle(string deviceInstanceId)
        {
            string[] interfaces = cmGetDeviceInterfaces(deviceInstanceId, GUID_DEVINTERFACE.GUID_DEVINTERFACE_HID);
             if (interfaces.Length > 0)
            {
                return CreateFile(interfaces[0], EFileAccess.FILE_GENERIC_WRITE | EFileAccess.FILE_GENERIC_READ, EFileShare.None, IntPtr.Zero, ECreationDisposition.OpenExisting, EFileAttributes.Device, IntPtr.Zero);
            }
            return null;
        }

        public static string hidGetSerialNumberString(SafeFileHandle h, int bufSize = 1024)
        {
            var buf = new byte[bufSize];

            if (HidD_GetSerialNumberString(h, buf, buf.Length))
            {
                return Encoding.Unicode.GetString(buf).Trim('\0');
            }
            throw new BetterWin32Errors.Win32Exception();
        }

        public static bool hidWriteFeature(SafeFileHandle h, byte[] report)
        {
            if (report == null) throw new ArgumentNullException(nameof(report));

            return HidD_SetFeature(h, report, report.Length);
        }



    }

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
}
