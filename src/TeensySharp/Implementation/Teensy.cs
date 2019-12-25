using HidLibrary;
using MoreLinq;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;


namespace lunOptics.TeensySharp.Implementation
{
    internal class Teensy : ITeensy
    {
        #region Implementation of ITeensy --------------------------------
        public UsbType UsbType 
        {
            get => _usbType;
            internal set => SetProperty(ref _usbType, value);
        }
        private UsbType _usbType;

        public string description { get; private set; }

        public UsbSubType UsbSubType
        {
            get => _usbSubType;
            internal set => SetProperty(ref _usbSubType, value);
        }
        private UsbSubType _usbSubType;


        public PJRC_Board BoardType { get; internal set; }
        public uint Serialnumber { get; internal set; }
        public string Port { get; internal set; }
        public string BoardId
        {
            get
            {
                switch (BoardType)
                {
                    case PJRC_Board.Teensy_2: return $"Teensy 2 ({Serialnumber})";
                    case PJRC_Board.Teensy_2pp: return $"Teensy 2++ ({Serialnumber})";
                    case PJRC_Board.Teensy_LC: return $"Teensy LC ({Serialnumber})";
                    case PJRC_Board.Teensy_30: return $"Teensy 3.0 ({Serialnumber})";
                    case PJRC_Board.Teensy_31_2: return $"Teensy 3.2 ({Serialnumber})";
                    case PJRC_Board.Teensy35: return $"Teensy 3.5 ({Serialnumber})";
                    case PJRC_Board.Teensy36: return $"Teensy 3.6 ({Serialnumber})";
                    case PJRC_Board.Teensy40: return $"Teensy 4.0 ({Serialnumber})";
                    case PJRC_Board.unknown: return $"Unknown Board ({Serialnumber})";
                    default: return null;
                }
            }
        }

        public bool Reboot()
        { 
            switch (UsbType)
            {
                case UsbType.HalfKay:
                    return true;

                case UsbType.Serial:
                    using (ISerialPortStream port = new SerialPortStream(this.Port))
                    {
                        port.Open();
                        port.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
                    }
                    break;

                case UsbType.HID:
                    hidDevice.WriteFeatureData(new byte[] { 0x00, 0xA9, 0x45, 0xC2, 0x6B });
                    break;

                default:
                    return false; // Unsupported USB mode
            }

            HidDevice device = null;
            var Timeout = DateTime.Now + TimeSpan.FromSeconds(3);
            while (DateTime.Now < Timeout)  //Try for some time in case HalfKay is just booting up
            {
                var devices = HidDevices.Enumerate(0x16C0, 0x0478); // Get all boards with running HalfKay
                device = devices.FirstOrDefault(x => TeensySharp.GetSerialNumber(x, 16) == Serialnumber); // check if the correct one is online
                if (device != null) break;  // found it              
                Thread.Sleep(500);
            }
            if (device == null) return false; // Didn't find a HalfKayed board with requested serialnumber

            this.UsbType = UsbType.HalfKay;
            this.hidDevice = device;
            this.Port = "";

            return true;
        }

        public bool Reset()
        {
            Reboot();
            if (UsbType != UsbType.HalfKay) return false;

            var rebootReport = hidDevice.CreateReport();
            rebootReport.Data[0] = 0xFF;
            rebootReport.Data[1] = 0xFF;
            rebootReport.Data[2] = 0xFF;
            hidDevice.WriteReport(rebootReport);

            return true;
        }

        public bool Upload(IFirmware firmware, bool checkType = true, bool reset = true)
        {
            if (checkType && firmware.boardType != BoardType) return false;

            if (UsbType != UsbType.HalfKay)
            {
                Reboot();
            }
            if (UsbType != UsbType.HalfKay) return false; // wasn't able to start bootloader

            var BoardDef = BoardDefinitions[BoardType];
            uint addr = 0;

            //Slice the flash image in dataBlocks and transfer the blocks if they are not empty (!=0xFF)
            foreach (var dataBlock in firmware.Getimage().Batch((int)BoardDef.BlockSize))
            {
                if (dataBlock.Any(d => d != 0xFF) || addr == 0) //skip empty blocks but always write first block to erase chip
                {
                    var report = PrepareReport(addr, dataBlock.ToArray(), BoardDef);
                    if (hidDevice.WriteReport(report))  //if write fails (happens if teensy still busy) wait and retry once
                    {
                        Thread.Sleep(10);
                        if (!hidDevice.WriteReport(report)) return false;
                    }
                    Thread.Sleep(addr == 0 ? 100 : 1); // First block needs more time since it erases the complete chip
                }
                addr += BoardDef.BlockSize;
            }

            if (reset)
            {
                Reset();

                while (true)
                {
                    Thread.Sleep(50);
                    var newTeensy = (Teensy)TeensySharp.ConnectedBoards.Where(t => t.Serialnumber == Serialnumber && t.BoardType != PJRC_Board.unknown)
                        .FirstOrDefault();
                    if (newTeensy != null)
                    {
                        UsbType = newTeensy.UsbType;
                        hidDevice = newTeensy.hidDevice;
                        Port = newTeensy.Port;
                        return true;
                    }
                }
            }
            return true;
        }
        #endregion

        #region Implementation of INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        public bool isConnected { get; private set; }

        internal HidDevice hidDevice { get; set; } = null;

        private static HidReport PrepareReport(uint addr, byte[] data, BoardDefinition BoardDef)
        {
            var report = new HidReport((int)(BoardDef.BlockSize + BoardDef.DataOffset + 1));
            report.ReportId = 0;

            // Copy address bytes into report. Use function stored in board definition
            BoardDef.AddrCopy(report.Data, BitConverter.GetBytes(addr));

            // Copy datablock into report
            data.CopyTo(report.Data, BoardDef.DataOffset);

            return report;
        }

        private static Dictionary<PJRC_Board, BoardDefinition> BoardDefinitions = new Dictionary<PJRC_Board, BoardDefinition>()
        {
            { PJRC_Board.Teensy40, new BoardDefinition
            {
                MCU =       "IMXRT1062",
                FlashSize = 2048 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            { PJRC_Board.Teensy36, new BoardDefinition
            {
                MCU =       "MK66FX1M0",
                FlashSize = 1024 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            { PJRC_Board.Teensy35, new BoardDefinition
            {
                MCU=       "MK64FX512",
                FlashSize = 512 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            {PJRC_Board.Teensy_31_2, new BoardDefinition
            {
                MCU=       "MK20DX256",
                FlashSize = 256 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            {PJRC_Board.Teensy_30, new BoardDefinition
            {
                MCU=       "MK20DX128",
                FlashSize = 128 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            {PJRC_Board.Teensy_LC, new BoardDefinition
            {
                MCU =      "MK126Z64",
                FlashSize = 62 * 1024,
                BlockSize = 512,
                DataOffset= 64,
                AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            {PJRC_Board.Teensy_2pp, new BoardDefinition
            {
                MCU =      "AT90USB1286",
                FlashSize = 12 * 1024,
                BlockSize = 256,
                DataOffset= 2,
                AddrCopy = (rep,addr)=>{rep[0]=addr[1]; rep[1]=addr[2];}
            }},

            {PJRC_Board.Teensy_2, new BoardDefinition
            {
                MCU =      "ATMEGA32U4",
                FlashSize = 31 * 1024,
                BlockSize = 128,
                DataOffset= 2,
                AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1];}
            }}
        };
               

        private class BoardDefinition
        {
            public uint FlashSize;
            public uint BlockSize;
            public uint DataOffset;
            public string MCU;
            public Action<byte[], byte[]> AddrCopy;
        }
    }
}








