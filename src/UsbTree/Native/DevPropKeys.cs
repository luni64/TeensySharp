using lunOptics.LibUsbTree.Implementation;
using System;

namespace lunOptics.LibUsbTree
{
    internal static class DevicePropertyKeys
    {
        public static DEVPROPKEY DeviceInstanceID =      new DEVPROPKEY() { pid = 256, fmtid = new Guid(0x78c34fc8, 0x104a, 0x4aca, 0x9e, 0xa4, 0x52, 0x4d, 0x52, 0x99, 0x6e, 0x57) };
        public static DEVPROPKEY Name =                  new DEVPROPKEY() { pid = 10,  fmtid = new Guid(0xb725f130, 0x47ef, 0x101a, 0xa5, 0xf1, 0x02, 0x60, 0x8c, 0x9e, 0xeb, 0xac) };    
        public static DEVPROPKEY BusReportedDeviceDesc = new DEVPROPKEY() { pid = 4,   fmtid = new Guid(0x540b947e, 0x8b40, 0x45bc, 0xa8, 0xa2, 0x6a, 0x0b, 0x89, 0x4c, 0xbd, 0xa2) };

        private static Guid GUID_Relations =             new Guid(0x4340a6c5, 0x93fa, 0x4706, 0x97, 0x2c, 0x7b, 0x64, 0x80, 0x08, 0xa5, 0xa7);
        public static DEVPROPKEY Parent =                new DEVPROPKEY() { pid = 8,   fmtid = GUID_Relations };
        public static DEVPROPKEY Children =              new DEVPROPKEY() { pid = 9,   fmtid = GUID_Relations};
                                                                                                                
        private static Guid GUID_SPDRP =                 new Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0);
        public static DEVPROPKEY DeviceClass =           new DEVPROPKEY() { pid = 9,   fmtid = GUID_SPDRP };
        public static DEVPROPKEY DeviceDesc =            new DEVPROPKEY() { pid = 2,   fmtid = GUID_SPDRP };
        public static DEVPROPKEY ContainerID =           new DEVPROPKEY() { pid = 38,  fmtid = GUID_SPDRP };
        public static DEVPROPKEY DeviceClassGuid =       new DEVPROPKEY() { pid = 10,  fmtid = GUID_SPDRP };
    }
}
