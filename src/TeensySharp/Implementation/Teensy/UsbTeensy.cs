using libTeensySharp.Implementation;
using lunOptics.libUsbTree;
using lunOptics.LibUsbTree;
using System;
using System.Text.RegularExpressions;

namespace lunOptics.libTeensySharp
{
    public class UsbTeensy : UsbDevice
    {
        public int Serialnumber { get; private set; } = 0;

        public UsbType UsbType
        {
            get => _usbType;
            protected set => SetProperty(ref _usbType, value);
        }
        private UsbType _usbType;

        public string Port
        {
            get => _port;
            private set => SetProperty(ref _port, value);
        }
        private string _port;

        public PJRC_Board BoardType
        {
            get => _boardType;
            set => SetProperty(ref _boardType, value);
        }
        private PJRC_Board _boardType = PJRC_Board.unknown;

        public override void update(InfoNode info)
        {
            doUpdate(info);
        }

        private void doUpdate(InfoNode info)
        {
            base.update(info);
            if (!IsInterface)
            {
                Serialnumber = getSerialnumber(info);


                if (ClassGuid == GUID_DEVCLASS.HIDCLASS) UsbType = UsbType.HID;
                else if (ClassGuid == GUID_DEVCLASS.USB)
                {
                    Description = "HUB " + Description;
                    UsbType = UsbType.Hub;
                }
                else if (ClassGuid == GUID_DEVCLASS.PORTS)
                {
                    Match mPort = Regex.Match(Description, @".*\(([^)]+)\)", RegexOptions.IgnoreCase);
                    if (mPort.Success) Port = mPort.Groups[1].Value;
                    UsbType = UsbType.Serial;
                }
                else UsbType = UsbType.unknown;



                if (Pid == HalfKayPid)
                {
                    UsbType = UsbType.HalfKay;

                    //switch (info.children[0].hidUsage)
                    //{
                    //    case 0x24: BoardType = PJRC_Board.T4_0; break;
                    //    default: BoardType = PJRC_Board.unknown; break;
                    //}

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
                    case UsbType.Serial:
                        Description = prefix + $"({Port})";
                        break;
                    case UsbType.HID:
                        Description = prefix + "(HID)";
                        break;
                    case UsbType.HalfKay:
                        Description = prefix + "(Bootloader)";
                        break;
                    case UsbType.Hub:
                        Description = prefix + "(Composite)";
                        break;
                }
            }
            else //Interface
            {
                Description = $"({Mi}) - {ClassDescription} " + (UsbType == UsbType.Serial ? $"({Port})" : "");
            }
        }
        public UsbTeensy(InfoNode info) : base(info)
        {
            doUpdate(info);
        }

        //public UsbTeensy(UsbDeviceInfo2 instance) : base(instance)
        //{
        //    if (instance == null) throw new ArgumentNullException(nameof(instance));

        //    if (!IsInterface)
        //    {
        //        if (Pid == HalfKayPid)
        //        {
        //            UsbType = UsbTypes.HalfKay;
        //            Serialnumber = Convert.ToInt32(SerialnumberString, 16) * 10;

        //            switch(instance.children[0].hidUsage)
        //            {
        //                case 0x24: BoardType = PJRC_Board.T4_0; break;
        //                default: BoardType = PJRC_Board.unknown; break;
        //            }

        //        }
        //        else
        //        {
        //            Serialnumber = Convert.ToInt32(SerialnumberString, 10);
        //            switch (Rev)
        //            {
        //                case 0x0273: BoardType = PJRC_Board.T_LC; break;
        //                case 0x0274: BoardType = PJRC_Board.T3_0; break;
        //                case 0x0275: BoardType = PJRC_Board.T3_2; break;
        //                case 0x0276: BoardType = PJRC_Board.T3_5; break;
        //                case 0x0277: BoardType = PJRC_Board.T3_6; break;
        //                case 0x0279: BoardType = PJRC_Board.T4_0; break;
        //                default: BoardType = PJRC_Board.unknown; break;
        //            }
        //        }

        //        var prefix = $"Teensy {BoardType} - {Serialnumber} ";

        //        switch (UsbType)
        //        {
        //            case UsbTypes.Serial:
        //                Description = prefix + $"({Port})";
        //                break;
        //            case UsbTypes.HID:
        //                Description = prefix + "(HID)";
        //                break;
        //            case UsbTypes.HalfKay:
        //                Description = prefix + "(Bootloader)";
        //                break;
        //            case UsbTypes.Hub:
        //                Description = prefix + "(Composite)";
        //                break;
        //        }
        //    }
        //    else //Interface
        //    {
        //        Description = $"({Mi}) - {Class} " + (UsbType == UsbTypes.Serial ? $"({Port})" : "");
        //    }
        //}

        public async void Reboot()
        {
            await TTeensy.Reboot(this);
            Console.WriteLine(this.Description);
        }
        
        public override bool isEqual(InfoNode otherDevice)
        {
            if (UsbTeensy.IsTeensy(otherDevice) )
            {
                //return base.isEqual(otherDevice);
                return otherDevice.isInterface ? otherDevice.serNumStr == SnString : UsbTeensy.getSerialnumber(otherDevice) == Serialnumber;

            }
            return false;
        }


        public static bool IsTeensy(InfoNode info)
        {
            return  info?.vid == PjrcVid && info.pid >= UsbTeensy.PjrcMinPid && info.pid <= UsbTeensy.PjRcMaxPid;
        }

        public static int getSerialnumber(InfoNode info)
        {
            return info?.pid == HalfKayPid ? Convert.ToInt32(info.serNumStr, 16) * 10 : Convert.ToInt32(info.serNumStr, 10);
        }


        public static readonly int HalfKayPid = 0x478;
        public static readonly int PjrcVid = 0x16C0;
        public static readonly int PjrcMinPid = 0;
        public static readonly int PjRcMaxPid = 0x500;
    }


}



