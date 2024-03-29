﻿using lunOptics.libUsbTree;
using MoreLinq;
using RJCP.IO.Ports;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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

        public uint Serialnumber { get; private set; } = 0;
        public UsbType UsbType { get; private set; }

        public List<string> Ports { get; } = new List<string>();
        public PJRC_Board BoardType { get; private set; } = PJRC_Board.unknown;

        public async Task<ErrorCode> ResetAsync(TimeSpan? timeout = null)
        {
            Trace.WriteLine($"Resetting {Description}");
            TimeSpan timeOut = timeout ?? TimeSpan.FromSeconds(6.5);

            var result = await RebootAsync(timeOut); // try to reboot teensy
            if (result != ErrorCode.OK)
            {
                Trace.WriteLine($"Error: {result}");
                return result;
            }

            Trace.WriteLine($"Write magic bytes to bootloader");
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

                result = hidWriteReport(hidHandle, report) ? ErrorCode.OK : ErrorCode.ResetError;
            }
            if (result != ErrorCode.OK) return result;

            Trace.WriteLine("Wait for device");
            //DateTime end = DateTime.Now + timeOut;
            DateTime start = DateTime.Now;
            while (UsbType == UsbType.HalfKay && DateTime.Now - start < timeOut)
            {
                await Task.Delay(10);
            }
            Trace.WriteLine($"Device appeared ({UsbType}) in {(DateTime.Now - start).TotalMilliseconds} ms");

            return (DateTime.Now - start < timeOut) ? ErrorCode.OK : ErrorCode.ResetError;
        }
        public async Task<ErrorCode> RebootAsync(TimeSpan? timeout = null)
        {
            Trace.WriteLine($"Rebooting {Description}");
            TimeSpan timeOut = timeout ?? TimeSpan.FromSeconds(6.5);
            try
            {
                if (UsbType == UsbType.HalfKay)
                {
                    Trace.WriteLine("HalfKay already running");
                    return await Task.FromResult(ErrorCode.OK); // already rebooted  
                }
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

                Trace.WriteLine("Wait for halfkay");
                while (UsbType != UsbType.HalfKay && DateTime.Now - start < timeOut)
                {
                    await Task.Delay(100);
                    Trace.WriteLine($"wait...{UsbType}");
                }
                Trace.WriteLine($"HalfKay found {UsbType} in {(DateTime.Now - start).TotalMilliseconds} ms");

                return (UsbType == UsbType.HalfKay) ? ErrorCode.OK : ErrorCode.RebootError;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
                return ErrorCode.Unexpected;
            }
        }
        public async Task<ErrorCode> UploadAsync(string hexFile, IProgress<int> progress = null, bool reboot = true, TimeSpan? timeout = null)
        {
            Trace.WriteLine($"Board type: {BoardType}");
            if (BoardType == PJRC_Board.unknown)
            {
                Trace.WriteLine("Start Bootloader to find information about the connected board");
                var r = await RebootAsync(timeout);    // try to start the bootloader
            }


            Trace.WriteLine($"Uploading: {hexFile} to {Description}");
            var timeOut = timeout ?? TimeSpan.FromSeconds(6.5);
            var firmware = new TeensyFirmware(hexFile);

            Trace.WriteLine($"Firmware type {firmware.boardType} Board type: {BoardType}");

            PJRC_Board boardType;
            if (BoardType == PJRC_Board.T4_1 || BoardType == PJRC_Board.T_MM) boardType = PJRC_Board.T4_0;
            else boardType = BoardType;
            // T4.1 and T4.0 have identical firmware
            //var boardType = (BoardType == PJRC_Board.T4_1) ? PJRC_Board.T4_0 : BoardType;

           

            if (firmware.boardType != boardType) return ErrorCode.Upload_FirmwareMismatch;

            var result = await RebootAsync(timeout);    // try to start the bootloader
            if (result != ErrorCode.OK) return result;
            if (boardType == PJRC_Board.unknown) return ErrorCode.ResetError;

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
                    var blocks = firmware.Getimage()
                        .Batch((int)boardDef.BlockSize)
                        .ToArray();

                    double progressDelta = 100.0 / blocks.Where((b, idx) => idx == 0 || !b.All(d => d == 0xFF)).Count();
                    double p = progressDelta;

                    uint addr = 0;
                    foreach (byte[] block in blocks)   //Slice the flash image in dataBlocks and transfer the blocks if they are not empty (!=0xFF)
                    {
                        if (addr == 0 || !block.All(d => d == 0xFF))          // skip empty blocks but always write first block to erase chip
                        {
                            Trace.Write($"Upload block at {addr} ");
                            Array.Clear(report, 0, report.Length);
                            BitConverter.GetBytes(addr).CopyTo(report, 1);   // Address starts at report[1] (report[0] = hid report number (0))
                            block.CopyTo(report, boardDef.DataOffset + 1);   // Copy datablock into report 
                            block.CopyTo(report, boardDef.DataOffset + 1);   // Copy datablock into report 

                            bool OK = false;
                            if (!hidWriteReport(hidHandle, report))           // if write fails (happens if teensy still busy) wait and retry 10 times max
                            {
                                for (int i = 0; i < 250; i++) 
                                {
                                    Trace.Write($"retry {i} ");
                                    await Task.Delay(100);
                                    if (hidWriteReport(hidHandle, report))
                                    {
                                        OK = true;
                                        break;
                                    }
                                }
                                if (!OK)
                                {
                                    result = ErrorCode.Upload_Timeout;
                                    Trace.WriteLine("upload failed, timeout writing block");
                                    break;
                                }
                            }
                            Trace.WriteLine("OK");

                            await Task.Delay(addr == 0 ? 500 : 1); // First block needs more time since it erases the complete chip
                            progress?.Report((int)p);
                            p += progressDelta;
                        }
                        addr += boardDef.BlockSize;
                    }
                }
            }

            progress?.Report(100);
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
            ErrorCode result = ErrorCode.OK;
            Task.Run(async () =>
            {
                result = await ResetAsync(timeout);
            });


            return result;


        }
        public ErrorCode Upload(string hexFile, bool reboot = true, TimeSpan? timeout = null)
        {
            var task = UploadAsync(hexFile, null, reboot, timeout);
            task.Wait();
            return task.Result;
        }

        public ErrorCode CheckPort()
        {
            if (!Ports.Any()) return ErrorCode.NoSerial;

            var port = new SerialPortStream(Ports.FirstOrDefault());

            ErrorCode result;
            try
            {
                port.Open();
                result = ErrorCode.OK;
            }
            catch (Exception e)
            {
                if (e is System.UnauthorizedAccessException)
                    result = ErrorCode.SerialBlocked;
                else
                    result = ErrorCode.Unexpected;
            }
            finally
            {
                port.Close();
            }
            return result;
        }

        public override bool isEqual(InfoNode otherDevice)
        {
            if (Teensy.IsTeensy(otherDevice))
            {
                //Trace.Write("isEqual Teensy ");
                //bool sameSN = false;
                //if (otherDevice.isInterface || otherDevice.isUsbFunction)
                //{

                //}
                //else
                //{
                //    var otherSN = getSerialnumber(otherDevice);
                //    sameSN = otherSN == Serialnumber;
                //}

                return (otherDevice.isInterface || otherDevice.isUsbFunction) ? otherDevice.serNumStr == SnString : getSerialnumber(otherDevice) == Serialnumber;
                //bool sameFunction = otherDevice.children 
                //Trace.WriteLine(sameSN);
                //return sameSN;
            }
            return false;
        }
        public override void update(InfoNode info) => doUpdate(info);
        internal static bool IsTeensy(InfoNode info) => info?.vid == PjrcVid && info.pid >= Teensy.PjrcMinPid && info.pid <= Teensy.PjRcMaxPid;
        protected static uint getSerialnumber(InfoNode info)
        {
            if (info?.pid == HalfKayPid)
            {
                UInt64 sn = Convert.ToUInt64(info.serNumStr, 16) * 10;
                if (sn > 0xFFFF_FFFF) sn = 0xFFFF_FFFF;  // handle home brew boards with sn = 0xffffffff
                return (UInt32)sn;
            }
            return Convert.ToUInt32(info.serNumStr, 10);
        }

        protected static int HalfKayPid => 0x478;
        protected static int PjrcVid => 0x16C0;
        protected static int PjrcMinPid => 0;
        protected static int PjRcMaxPid => 0x500;
        protected static uint SerEmuUsageID => 0xFFC9_0004;
        protected static uint RawHidUsageID => 0xFFAB_0200;
        protected void doUpdate(InfoNode info)
        {
            // Trace.WriteLine("update Teensy");
            base.update(info);

            if (ClassGuid == GUID_DEVCLASS.HIDCLASS) UsbType = UsbType.HID;
            else if (ClassGuid == GUID_DEVCLASS.USB) UsbType = UsbType.COMPOSITE;
            else if (ClassGuid == GUID_DEVCLASS.MEDIA) UsbType = UsbType.Media;
            else if (ClassGuid == GUID_DEVCLASS.PORTS)
            {
                UsbType = UsbType.Serial;
                //Serialnumber = getSerialnumber(info);
                Ports.Clear();
                Match mPort = Regex.Match(Description, @".*\(([^)]+)\)", RegexOptions.IgnoreCase);
                if (mPort.Success) Ports.Add(mPort.Groups[1].Value);
            }

            else UsbType = UsbType.unknown;

            if (!IsInterface && !IsUsbFunction )
            {
                Serialnumber = getSerialnumber(info);

                if (Pid == HalfKayPid)
                {
                    UsbType = UsbType.HalfKay;
                    Ports.Clear();
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
                            case 0xFF9C_0026: BoardType = PJRC_Board.T_MM; break;

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
                        case 0x0281: BoardType = PJRC_Board.T_MM; break;
                        default: BoardType = PJRC_Board.unknown; break;
                    }
                }

                var prefix = $"Teensy {BoardType} - {Serialnumber} ";

                switch (UsbType)
                {
                    case UsbType.Serial: Description = prefix + $"({Ports.FirstOrDefault()})"; break;
                    case UsbType.HID: Description = prefix + "(HID)"; break;
                    case UsbType.HalfKay: Description = prefix + "(Bootloader)"; break;
                    case UsbType.COMPOSITE: Description = prefix + "(Composite)"; break;
                }
            }
            else //Interface
            {
                Description = $"({Mi}) - {ClassDescription} " + (UsbType == UsbType.Serial ? $"({Ports.FirstOrDefault() ?? "??"})" : "");
            }

            if (UsbType == UsbType.COMPOSITE)
            {
                Ports.Clear();
                foreach (Teensy function in functions.OfType<Teensy>().Where(f => f.UsbType == UsbType.Serial))
                {
                    Ports.Add(function.Ports.FirstOrDefault() ?? "??");
                }
            }
            OnPropertyChanged("");  // update all properties
        }

        protected static void rebootSerial(string portName)
        {
            Trace.WriteLine("rebootSerial");
            using (var p = new SerialPortStream(portName))
            {
                p.Open();
                p.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...   
                p.Close();
            }
        }

        protected static bool rebootSerEmu(IUsbDevice iface)
        {
            Trace.WriteLine("rebootSerEmu");
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
            { PJRC_Board.T_MM, new BoardDefinition
            {
                MCU =       "IMXRT1062",
                FlashSize = 8192 * 1024,
                BlockSize = 1024,
                DataOffset= 64,
            }},

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



