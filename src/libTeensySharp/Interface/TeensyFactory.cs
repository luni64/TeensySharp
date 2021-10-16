using lunOptics.libTeensySharp.Implementation;
using lunOptics.libUsbTree;

namespace lunOptics.libTeensySharp
{
    public class TeensyFactory : DeviceFactory
    {
        public override IUsbDevice newDevice(InfoNode info)
        {
            return Teensy.IsTeensy(info) ? new Teensy(info) : new UsbDevice(info);
        }
    }
}