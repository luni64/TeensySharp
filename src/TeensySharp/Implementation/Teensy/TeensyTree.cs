using lunOptics.LibUsbTree;
using System;

namespace lunOptics.TeensyTree.Implementation
{
    public class TeensyTree : UsbTree
    {
        protected override UsbDevice MakeDevice(UsbDeviceInfo instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            if (instance.vid == 0x16c0)
            {
                return new UsbTeensy(instance);
            }
            return base.MakeDevice(instance);
        }
    }
}
