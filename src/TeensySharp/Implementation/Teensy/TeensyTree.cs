using lunOptics.libUsbTree;
using System;
using System.Threading;
using static lunOptics.libTeensySharp.UsbTeensy;

namespace lunOptics.libTeensySharp
{
    public class TeensyTree : UsbTree
    {
        public TeensyTree(SynchronizationContext ctx) : base(ctx) { }

        public override UsbDevice MakeDevice(UsbDeviceInfo2 deviceInfo)
        {
            if (deviceInfo == null) throw new ArgumentNullException(nameof(deviceInfo));

            if (deviceInfo.vid == PjrcVid && deviceInfo.pid >= PjrcMinPid && deviceInfo.pid <= PjRcMaxPid)
            {
                return new UsbTeensy(deviceInfo);
            }
            return base.MakeDevice(deviceInfo);
        }
    }
}
