using HidLibrary;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Threading;
using TeensySharp.Interface;

namespace TeensySharp
{
    internal class Teensy_t : ITeensy
    {
        #region Implementation of ITeensy --------------------------------
        public USBtype usbType { get; internal set; }
        public PJRC_Board boardType { get; internal set; }
        public uint serialnumber { get; internal set; }
        public string port { get; internal set; }
        public string boardId
        {
            get
            {
                switch (boardType)
                {
                    case PJRC_Board.Teensy_2: return $"Teensy 2 ({serialnumber})";
                    case PJRC_Board.Teensy_2pp: return $"Teensy 2++ ({serialnumber})";
                    case PJRC_Board.Teensy_LC: return $"Teensy LC ({serialnumber})";
                    case PJRC_Board.Teensy_30: return $"Teensy 3.0 ({serialnumber})";
                    case PJRC_Board.Teensy_31_2: return $"Teensy 3.2 ({serialnumber})";
                    case PJRC_Board.Teensy_35: return $"Teensy 3.5 ({serialnumber})";
                    case PJRC_Board.Teensy_36: return $"Teensy 3.6 ({serialnumber})";
                    case PJRC_Board.Teensy_40: return $"Teensy 4.0 ({serialnumber})";
                    case PJRC_Board.unknown: return $"Unknown Board ({serialnumber})";
                    default: return null;
                }
            }
        }

        public bool StartBootloader()
        {
            switch (usbType)
            {
                case USBtype.HalfKay:
                    return true;

                case USBtype.UsbSerial:
                    using (var port = new SerialPort(this.port))
                    {
                        port.Open();
                        port.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
                    }
                    break;

                case USBtype.HID:
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
                device = devices.FirstOrDefault(x => TeensyWatcher.GetSerialNumber(x, 16) == serialnumber); // check if the correct one is online
                if (device != null) break;  // found it              
                Thread.Sleep(500);
            }
            if (device == null) return false; // Didn't find a HalfKayed board with requested serialnumber

            this.usbType = USBtype.HalfKay;
            this.hidDevice = device;
            this.port = "";

            return true;
        }
        public bool Upload(IFirmware firmware, bool checkType = true, bool reset = true)
        {
            if (checkType && firmware.boardType != boardType) return false; 

            if (usbType != USBtype.HalfKay)
            {
                StartBootloader();
            }
            if (usbType != USBtype.HalfKay) return false; // wasn't able to start bootloader

            var BoardDef = BoardDefinitions[boardType];
            uint addr = 0;

            //Slice the flash image in dataBlocks and transfer the blocks if they are not empty (!=0xFF)
            foreach (var dataBlock in firmware.image.Batch((int)BoardDef.BlockSize))
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
                    var newTeensy = (Teensy_t)TeensyWatcher.getConnectedTeensies().Where(t => t.serialnumber == serialnumber && t.boardType != PJRC_Board.unknown).FirstOrDefault();
                    if (newTeensy != null)
                    {
                        usbType = newTeensy.usbType;
                        hidDevice = newTeensy.hidDevice;
                        port = newTeensy.port;
                        return true;
                    }
                }
            }
            return true;
        }
        public bool Reset()
        {
            StartBootloader();
            if (usbType != USBtype.HalfKay) return false;

            var rebootReport = hidDevice.CreateReport();
            rebootReport.Data[0] = 0xFF;
            rebootReport.Data[1] = 0xFF;
            rebootReport.Data[2] = 0xFF;
            hidDevice.WriteReport(rebootReport);

            return true;
        }
        #endregion

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
            { PJRC_Board.Teensy_40, new BoardDefinition
            {
                MCU =       "IMXRT1062",
                FlashSize = 2048 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            { PJRC_Board.Teensy_36, new BoardDefinition
            {
                MCU =       "MK66FX1M0",
                FlashSize = 1024 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
                AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
            }},

            { PJRC_Board.Teensy_35, new BoardDefinition
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


    //internal class BoardDefinition
    //{
    //    public uint FlashSize;
    //    public uint BlockSize;
    //    public uint DataOffset;
    //    public string MCU;
    //    public Action<byte[], byte[]> AddrCopy;
    //}

    //static class BD
    //{
    //    public BoardDefinition  this[int i]
    //    {

    //    }

    //    static Dictionary<PJRC_Board, BoardDefinition> bd = new Dictionary<PJRC_Board, BoardDefinition>
    //    {
    //        { PJRC_Board.Teensy_40, new BoardDefinition
    //        {
    //            MCU =       "IMXRT1062",
    //            FlashSize = 2048 * 1024,
    //            BlockSize = 1024,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        { PJRC_Board.Teensy_36, new BoardDefinition
    //        {
    //            MCU =       "MK66FX1M0",
    //            FlashSize = 1024 * 1024,
    //            BlockSize = 1024,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        { PJRC_Board.Teensy_35, new BoardDefinition
    //        {
    //            MCU=       "MK64FX512",
    //            FlashSize = 512 * 1024,
    //            BlockSize = 1024,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        {PJRC_Board.Teensy_31_2, new BoardDefinition
    //        {
    //            MCU=       "MK20DX256",
    //            FlashSize = 256 * 1024,
    //            BlockSize = 1024,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr) => {rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        {PJRC_Board.Teensy_30, new BoardDefinition
    //        {
    //            MCU=       "MK20DX128",
    //            FlashSize = 128 * 1024,
    //            BlockSize = 1024,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        {PJRC_Board.Teensy_LC, new BoardDefinition
    //        {
    //            MCU =      "MK126Z64",
    //            FlashSize = 62 * 1024,
    //            BlockSize = 512,
    //            DataOffset= 64,
    //            AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1]; rep[2]=addr[2];}
    //        }},
    //        {PJRC_Board.Teensy_2pp, new BoardDefinition
    //        {
    //            MCU =      "AT90USB1286",
    //            FlashSize = 12 * 1024,
    //            BlockSize = 256,
    //            DataOffset= 2,
    //            AddrCopy = (rep,addr)=>{rep[0]=addr[1]; rep[1]=addr[2];}
    //        }},
    //        {PJRC_Board.Teensy_2, new BoardDefinition
    //        {
    //            MCU =      "ATMEGA32U4",
    //            FlashSize = 31 * 1024,
    //            BlockSize = 128,
    //            DataOffset= 2,
    //            AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1];}
    //        }}
    //    };

    //    class BoardDefinition
    //    {
    //        public uint FlashSize;
    //        public uint BlockSize;
    //        public uint DataOffset;
    //        public string MCU;
    //        public Action<byte[], byte[]> AddrCopy;
    //    }
    //};
}








