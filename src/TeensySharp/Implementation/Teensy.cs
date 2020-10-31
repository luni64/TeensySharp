using lunOptics.libUsbTree;
using lunOptics.libUsbTree.Implementation;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static lunOptics.libTeensySharp.Native.NativeWrapper;

namespace lunOptics.libTeensySharp.Implementation
{
    internal class Teensy : UsbDevice, ITeensy
    {
        public Teensy(InfoNode info) : base(info)
        {
            doUpdate(info);
        }

        public int Serialnumber { get; private set; } = 0;
        public UsbType UsbType { get; private set; }
    
        public List<string> Ports { get; } = new List<string>();
        public PJRC_Board BoardType { get; private set; }
        
        public async Task<ErrorCode> ResetAsync(TimeSpan? timeout = null)
        {
            TimeSpan timeOut = timeout ?? TimeSpan.FromSeconds(6.5);

            var result = await RebootAsync(timeOut); // try to reboot teensy
            if (result != ErrorCode.OK) return result;

            using (var hidHandle = getFileHandle(interfaces[0].DeviceInstanceID))
            {
                if (hidHandle == null || hidHandle.IsInvalid) return ErrorCode.HidComError;

                var boardDef = BoardDefinitions[BoardType];                
                var reportSize = boardDef.BlockSize + boardDef.DataOffset + 1;

                var report = new byte[reportSize];
                report[0] = 0x00;
                report[1] = 0xFF;
                report[2] = 0xFF;
                report[3] = 0xFF;

                return hidWriteReport(hidHandle, report) ? ErrorCode.OK : ErrorCode.ResetError;
            }
        }
        public async Task<ErrorCode> RebootAsync(TimeSpan? timeout = null)
        {
            TimeSpan timeOut = timeout ?? TimeSpan.FromSeconds(6.5);
            try
            {
                if (UsbType == UsbType.HalfKay) return await Task.FromResult(ErrorCode.OK); // already rebooted  
                if (UsbType == UsbType.Serial) rebootSerial(Ports[0]);                      // simple serial device, can be rebooted directly
                else                                                                        // composite device, check for a rebootable function
                {
                    foreach (Teensy usbFunction in functions)
                    {
                        if (usbFunction.UsbType == UsbType.Serial)                          // serial   
                        {
                            rebootSerial(usbFunction.Ports[0]);
                            break;
                        }
                        else                                                                // check for seremu
                        {
                            var iface = usbFunction.interfaces.FirstOrDefault(i => i.HidUsageID == Teensy.SerEmuUsageID);
                            if (iface != null)
                            {
                                rebootSerEmu(iface);
                                break;
                            }
                        }
                    }
                }

                // wait until teensy appears as bootloader 
                var start = DateTime.Now;
                while (UsbType != UsbType.HalfKay && DateTime.Now - start < timeOut)
                {
                    await Task.Delay(10);
                }
                return (UsbType == UsbType.HalfKay) ? ErrorCode.OK : ErrorCode.RebootError;
            }
            catch 
            {
                return ErrorCode.Unexpected;
            }
        }
        public async Task<ErrorCode> UploadAsync(string hexFile, bool reboot = true, TimeSpan? timeout = null)
        {
            TimeSpan timeOut = timeout ?? TimeSpan.FromSeconds(6.5);
            IFirmware firmware = new TeensyFirmware(hexFile);

            var boardType = (BoardType == PJRC_Board.T4_1) ? PJRC_Board.T4_0 : BoardType;
            if (firmware.boardType != boardType) return ErrorCode.Upload_FirmwareMismatch;


            var result = await RebootAsync(timeout);    // try to start the bootloader
            if (result != ErrorCode.OK) return result;

            var boardDef = BoardDefinitions[BoardType];
            var report = new byte[boardDef.BlockSize + boardDef.DataOffset + 1];

            using (var hidHandle = getFileHandle(interfaces[0].DeviceInstanceID))   // open communication channel to the bootloader
            {
                if (hidHandle == null || hidHandle.IsInvalid)
                {
                    result = ErrorCode.HidComError;
                }
                else
                {
                    uint addr = 0;
                    foreach (byte[] dataBlock in firmware.Getimage().Batch((int)boardDef.BlockSize).ToArray())   //Slice the flash image in dataBlocks and transfer the blocks if they are not empty (!=0xFF)
                    {
                        if (addr == 0 || !dataBlock.All(d => d == 0xFF))        //skip empty blocks but always write first block to erase chip
                        {
                            Array.Clear(report, 0, report.Length);
                            BitConverter.GetBytes(addr).CopyTo(report, 1);      // Address starts at report[1] (report[0] = hid report number (0))
                            dataBlock.CopyTo(report, boardDef.DataOffset + 1);  // Copy datablock into report 

                            if (hidWriteReport(hidHandle, report))              //if write fails (happens if teensy still busy) wait and retry once
                            {
                                await Task.Delay(10);
                                if (!hidWriteReport(hidHandle, report))
                                {
                                    result = ErrorCode.Upload_Timeout;
                                    break;
                                }
                            }
                            await Task.Delay(addr == 0 ? 100 : 1); // First block needs more time since it erases the complete chip
                        }
                        addr += boardDef.BlockSize;
                    }
                }
            }

            if (result == ErrorCode.OK && reboot)
            {
                result = await ResetAsync(timeOut);
            }
            return result;
        }
        public ErrorCode Reboot(TimeSpan? timeout = null)
        {
            var task = RebootAsync(timeout);
            task.Wait();
            return task.Result;
        }
        public ErrorCode Reset(TimeSpan? timeout = null)
        {
            var task = ResetAsync(timeout);
            task.Wait();
            return task.Result;
        }
        public ErrorCode Upload(string hexFile, bool reboot = true, TimeSpan? timeout = null)
        {
            var task = UploadAsync(hexFile, reboot, timeout);
            task.Wait();
            return task.Result;
        }

        public override bool isEqual(InfoNode otherDevice)
        {
            if (Teensy.IsTeensy(otherDevice))
            {
                return (otherDevice.isInterface || otherDevice.isUsbFunction)
                    ? otherDevice.serNumStr == SnString
                    : Teensy.getSerialnumber(otherDevice) == Serialnumber;
            }
            return false;
        }
        public override void update(InfoNode info)
        {
            doUpdate(info);
        }
        internal static bool IsTeensy(InfoNode info)
        {
            return info?.vid == PjrcVid && info.pid >= Teensy.PjrcMinPid && info.pid <= Teensy.PjRcMaxPid;
        } 
        protected static int getSerialnumber(InfoNode info)
        {
            return info?.pid == HalfKayPid ? Convert.ToInt32(info.serNumStr, 16) * 10 : Convert.ToInt32(info.serNumStr, 10);
        }

        protected static int HalfKayPid => 0x478;
        protected static int PjrcVid => 0x16C0;
        protected static int PjrcMinPid => 0;
        protected static int PjRcMaxPid => 0x500;
        protected static uint SerEmuUsageID => 0xFFC9_0004;
        protected static uint RawHidUsageID => 0xFFAB_0200;
        protected void doUpdate(InfoNode info)
        {
            base.update(info);

            if (ClassGuid == GUID_DEVCLASS.HIDCLASS) UsbType = UsbType.HID;
            else if (ClassGuid == GUID_DEVCLASS.USB) UsbType = UsbType.COMPOSITE;
            else if (ClassGuid == GUID_DEVCLASS.PORTS)
            {
                UsbType = UsbType.Serial;
                Match mPort = Regex.Match(Description, @".*\(([^)]+)\)", RegexOptions.IgnoreCase);
                if (mPort.Success) Ports.Add(mPort.Groups[1].Value);


            }
            else UsbType = UsbType.unknown;

            if (!IsInterface && !IsUsbFunction)
            {
                Serialnumber = getSerialnumber(info);

                if (Pid == HalfKayPid)
                {
                    UsbType = UsbType.HalfKay;
                    if (interfaces.Count > 0)
                    {
                        var iface = interfaces[0] as Teensy;
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
                            case 0xFF9C_0025: BoardType = PJRC_Board.T4_1; break;

                            default: BoardType = PJRC_Board.unknown; break;
                        };
                    }
                    else BoardType = PJRC_Board.unknown;
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
                        case 0x0280: BoardType = PJRC_Board.T4_1; break;
                        default: BoardType = PJRC_Board.unknown; break;
                    }
                }

                var prefix = $"Teensy {BoardType} - {Serialnumber} ";

                switch (UsbType)
                {
                    case UsbType.Serial: Description = prefix + $"({Ports[0]})"; break;
                    case UsbType.HID: Description = prefix + "(HID)"; break;
                    case UsbType.HalfKay: Description = prefix + "(Bootloader)"; break;
                    case UsbType.COMPOSITE: Description = prefix + "(Composite)"; break;
                }
            }
            else //Interface
            {
                Description = $"({Mi}) - {ClassDescription} " + (UsbType == UsbType.Serial ? $"({Ports[0]})" : "");
            }
          
            if(UsbType == UsbType.COMPOSITE)
            {
                Ports.Clear();
                foreach (Teensy function in functions.OfType<Teensy>().Where(f => f.UsbType == UsbType.Serial))
                {
                    Ports.Add(function.Ports[0]);
                }
            }

            OnPropertyChanged("");  // update all properties
        }

        //  private string Port { get; set; }

        protected static void rebootSerial(string portName)
        {
            using (var p = new SerialPort(portName))
            {
                p.Open();
                p.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
            }
        }

        protected static bool rebootSerEmu(IUsbDevice iface)
        {
            var hidHandle = getFileHandle(iface.DeviceInstanceID);
            if (hidHandle?.IsInvalid ?? true)
            {
                hidHandle?.Dispose();
                return false;
            }

            bool result = hidWriteFeature(hidHandle, new byte[] { 0x00, 0xA9, 0x45, 0xC2, 0x6B });
            hidHandle.Dispose();
            return result;
        }

        protected class BoardDefinition
        {
            public uint FlashSize;
            public uint BlockSize;
            public uint DataOffset;
            public string MCU;         
        }
        protected static readonly Dictionary<PJRC_Board, BoardDefinition> BoardDefinitions = new Dictionary<PJRC_Board, BoardDefinition>()
        {
            { PJRC_Board.T4_1, new BoardDefinition
            {
                MCU =       "IMXRT1062",
                FlashSize = 2048 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            { PJRC_Board.T4_0, new BoardDefinition
            {
                MCU =       "IMXRT1062",
                FlashSize = 2048 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            { PJRC_Board.T3_6, new BoardDefinition
            {
                MCU =       "MK66FX1M0",
                FlashSize = 1024 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            { PJRC_Board.T3_5, new BoardDefinition
            {
                MCU=       "MK64FX512",
                FlashSize = 512 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            {PJRC_Board.T3_2, new BoardDefinition
            {
                MCU=       "MK20DX256",
                FlashSize = 256 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            {PJRC_Board.T3_0, new BoardDefinition
            {
                MCU=       "MK20DX128",
                FlashSize = 128 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

            {PJRC_Board.T_LC, new BoardDefinition
            {
                MCU =      "MK126Z64",
                FlashSize = 62 * 1024,
                BlockSize = 512,
                DataOffset= 64,
            }},

            //{PJRC_Board.Teensy_2pp, new BoardDefinition
            //{
            //    MCU =      "AT90USB1286",
            //    FlashSize = 12 * 1024,
            //    BlockSize = 256,
            //    DataOffset= 2,
            //    AddrCopy = (rep,addr)=>{rep[0]=addr[1]; rep[1]=addr[2];}
            //}},

            //{PJRC_Board.Teensy_2, new BoardDefinition
            //{
            //    MCU =      "ATMEGA32U4",
            //    FlashSize = 31 * 1024,
            //    BlockSize = 128,
            //    DataOffset= 2,
            //    AddrCopy = (rep,addr)=>{rep[0]=addr[0]; rep[1]=addr[1];}
            //}}
        };
    }
}



