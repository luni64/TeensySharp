using lunOptics.LibUsbTree;
using System;

namespace lunOptics.TeensyTree.Implementation
{
    public class UsbTeensy : UsbDevice
    {
        public UsbTeensy(UsbDeviceInfo instance) : base(instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            int sn;
            if (!instance.isInterface)
            {
                sn = pid == HalfKayPid ?  Convert.ToInt32(SerialnumberString, 16)*10 : Convert.ToInt32(SerialnumberString, 10);

            }
            else sn = 0; 


            

            Description = $"Teensy ({pid:X}) SN: {sn}";

        }


        public const int HalfKayPid = 0x478;
        public const int PjrcVid = 0x16C0;
        public const int PjrcMinPid = 0;
        public const int PjRcMaxPid = 0x500;
    }
}

//public class UsbSerial : UsbDevice, IUsbSerial
//{
//    internal UsbSerial(DeviceInstance inst) : base(inst)
//    {
//        Port = null;
//    }
//    public string Port { get; set; }

//    public override string ToString()
//    {
//        return $"{Description} {Port} ({vid:X4}/{pid:X4}) #{SerialnumberString}";
//    }
//}
