using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;

namespace libTeensySharp.Implementation.Teensy
{
    public class TeensyFactory : DeviceFactory
    {
        public override UsbDevice newDevice(InfoNode info)
        {
            return UsbTeensy.IsTeensy(info) ? new UsbTeensy(info) : new UsbDevice(info);
        }
    }
}