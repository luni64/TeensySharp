using System;

namespace lunOptics.UsbTree.Implementation
{
    public static class DevPropKeys
    {
        public static DEVPROPKEY DeviceInstanceId;
        public static DEVPROPKEY DeviceClass;
        public static DEVPROPKEY DeviceClassGuid;
        public static DEVPROPKEY BusReportedDeviceDesc;
        public static DEVPROPKEY DeviceDesc;
        public static DEVPROPKEY FriendlyName;
        public static DEVPROPKEY ContainerID;
        public static DEVPROPKEY Children;
        public static DEVPROPKEY Parent;


        static DevPropKeys()
        {
            var SPDRP_GUID = new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);

            DeviceDesc = new DEVPROPKEY() { pid = 2, fmtid = SPDRP_GUID };
            ContainerID = new DEVPROPKEY() { pid = 38, fmtid = SPDRP_GUID };
            DeviceClass = new DEVPROPKEY() { pid = 9, fmtid = SPDRP_GUID };
            DeviceClassGuid = new DEVPROPKEY() { pid = 10, fmtid = SPDRP_GUID };

            DeviceInstanceId = new DEVPROPKEY() { pid = 256, fmtid = new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57) };
            BusReportedDeviceDesc = new DEVPROPKEY() { pid = 4, fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2) };

            Parent = new DEVPROPKEY() { pid = 8, fmtid = new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7) };
            Children = new DEVPROPKEY() { pid = 9, fmtid = new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7) };
            FriendlyName = new DEVPROPKEY() { pid = 14, fmtid = new Guid(0x026e516e, 0xb814, 0x414b, 0x83, 0xcd, 0x85, 0x6d, 0x6f, 0xef, 0x48, 0x22) };
        }
    }
}
