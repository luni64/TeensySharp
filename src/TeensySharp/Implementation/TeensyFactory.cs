using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;

namespace libTeensySharp.Implementation.Teensy
{
    public class TeensyFactory : DeviceFactory
    {
        public override UsbDevice newDevice(InfoNode info)
        {
            return lunOptics.libTeensySharp.Teensy.IsTeensy(info) ? new lunOptics.libTeensySharp.Teensy(info) : new UsbDevice(info);
        }
    }
}