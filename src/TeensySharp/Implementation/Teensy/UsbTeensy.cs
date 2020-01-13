using libTeensySharp.Implementation;
using lunOptics.libUsbTree;
using System;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;

namespace lunOptics.libTeensySharp
{
    public class UsbTeensy : UsbDevice
    {
        public int Serialnumber { get; private set; } = 0;
        //public uint usageId { get; private set; }
        //public int usagepage { get; private set; }
        public UsbType UsbType { get; private set; }
        public string Port { get; private set; }
        public PJRC_Board BoardType { get; private set; }
        
        public override void update(InfoNode info)
        {
            doUpdate(info);
        }

        private void doUpdate(InfoNode info)
        {
            base.update(info);

            if (ClassGuid == GUID_DEVCLASS.HIDCLASS) UsbType = UsbType.HID;
            else if (ClassGuid == GUID_DEVCLASS.USB) UsbType = UsbType.COMPOSITE;
            else if (ClassGuid == GUID_DEVCLASS.PORTS)
            {
                UsbType = UsbType.Serial;
                Match mPort = Regex.Match(Description, @".*\(([^)]+)\)", RegexOptions.IgnoreCase);
                if (mPort.Success) Port = mPort.Groups[1].Value;
            }
            else UsbType = UsbType.unknown;

            if (!IsInterface && !IsUsbFunction)
            {
                Serialnumber = getSerialnumber(info);

                if (Pid == HalfKayPid)
                {
                    UsbType = UsbType.HalfKay;
                    //if (interfaces.Count > 0) 
                    //{
                    var iface = interfaces[0] as UsbTeensy;
                    switch (iface.HidUsageID)
                    {                       
                        case 0xFF9C_001B: BoardType = PJRC_Board.Teensy_2; break;
                        case 0xFF9C_001C: BoardType = PJRC_Board.Teensy_2pp; break;
                        case 0xFF9C_001D: BoardType = PJRC_Board.T3_0; break;
                        case 0xFF9C_001E: BoardType = PJRC_Board.T3_2; break;
                        case 0xFF9C_0020: BoardType = PJRC_Board.T_LC; break;
                        case 0xFF9C_0021: BoardType = PJRC_Board.T3_2; break;
                        case 0xFF9C_001F: BoardType = PJRC_Board.T3_5; break;
                        case 0xFF9C_0022: BoardType = PJRC_Board.T3_6; break;
                        case 0xFF9C_0024: BoardType = PJRC_Board.T4_0; break;
                        default: BoardType = PJRC_Board.unknown; break;
                    };
                    //}
                    //else BoardType = PJRC_Board.unknown;
                }
                else
                {
                    switch (Rev)
                    {
                        case 0x0273: BoardType = PJRC_Board.T_LC; break;
                        case 0x0274: BoardType = PJRC_Board.T3_0; break;
                        case 0x0275: BoardType = PJRC_Board.T3_2; break;
                        case 0x0276: BoardType = PJRC_Board.T3_5; break;
                        case 0x0277: BoardType = PJRC_Board.T3_6; break;
                        case 0x0279: BoardType = PJRC_Board.T4_0; break;
                        default: BoardType = PJRC_Board.unknown; break;
                    }
                }

                var prefix = $"Teensy {BoardType} - {Serialnumber} ";

                switch (UsbType)
                {
                    case UsbType.Serial: Description = prefix + $"({Port})"; break;
                    case UsbType.HID: Description = prefix + "(HID)"; break;
                    case UsbType.HalfKay: Description = prefix + "(Bootloader)"; break;
                    case UsbType.COMPOSITE: Description = prefix + "(Composite)"; break;
                }
            }
            else //Interface
            {
                Description = $"({Mi}) - {ClassDescription} " + (UsbType == UsbType.Serial ? $"({Port})" : "");
            }
            OnPropertyChanged("");  // update all properties
        }
        public UsbTeensy(InfoNode info) : base(info)
        {
            doUpdate(info);
        }

        public async Task<bool> ResetAsync()
        {
            return await TTeensy.ResetAsync(this);
        }

        public async Task<bool> RebootAsync()
        {
            return await TTeensy.RebootAsync(this);
        }

        public bool Reboot()
        {
            var task = TTeensy.RebootAsync(this);
            task.Wait();
            return task.Result;
        }

        public override bool isEqual(InfoNode otherDevice)
        {
            if (UsbTeensy.IsTeensy(otherDevice))
            {         
                return (otherDevice.isInterface || otherDevice.isUsbFunction) 
                    ? otherDevice.serNumStr == SnString 
                    : UsbTeensy.getSerialnumber(otherDevice) == Serialnumber;
            }
            return false;
        }


        public static bool IsTeensy(InfoNode info)
        {
            return info?.vid == PjrcVid && info.pid >= UsbTeensy.PjrcMinPid && info.pid <= UsbTeensy.PjRcMaxPid;
        }

        public static int getSerialnumber(InfoNode info)
        {
            return info?.pid == HalfKayPid ? Convert.ToInt32(info.serNumStr, 16) * 10 : Convert.ToInt32(info.serNumStr, 10);
        }

        public static readonly int HalfKayPid = 0x478;
        public static readonly int PjrcVid = 0x16C0;
        public static readonly int PjrcMinPid = 0;
        public static readonly int PjRcMaxPid = 0x500;
        public static readonly uint SerEmuUsageID = 0xFFC9_0004;
        public static readonly uint RawHidUsageID = 0xFFAB_0200;
    }


}



