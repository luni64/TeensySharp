﻿using HidLibrary;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace TeensySharp
{
    /// <summary>
    /// This class handles uploading of firmware images to the PJRC Teensy boards. 
    /// <para>
    /// Workflow: 
    /// <para> 1) Get an empty firmware image by calling GetEmptyFirmwareImage. </para>
    /// <para> 2) Copy the bytes to be programmed into the image (either manually :-) or using the SharpHexFileParser)</para>
    /// <para> 3) Call "Upload" to programm the chip</para>    
    /// </para>
    /// </summary>
    public static class SharpUploader
    {
        #region Public Methods ----------------------------------------------------------------------------------

        public static int Upload(byte[] Image, PJRC_Board board, uint Serialnumber, bool reboot = true)
        {
            // Obtain a HalfKayed board with the required serialnumber        
            HidDevice device = null;
            var Timeout = DateTime.Now + TimeSpan.FromSeconds(3);
            while (DateTime.Now < Timeout)  //Try for some time in case HalfKay is just booting up
            {
                var devices = HidDevices.Enumerate(0x16C0, 0x0478); // Get all boards with running HalfKay
                device = devices.FirstOrDefault(x => GetSerialNumber(x) == Serialnumber); // check if the correct one is online
                if (device != null) break;  // found it              
                Thread.Sleep(500);
            }
            if (device == null) return 1; // Didn't find a HalfKayed board with requested serialnumber


            var BoardDef = BoardDefinitions[board];
            int addr = 0;

            //Slice the flash image in dataBlocks and transfer the blocks if they are not empty (!=0xFF)
            foreach (var dataBlock in Image.Batch(BoardDef.BlockSize))
            {
                if (dataBlock.Any(d => d != 0xFF) || addr == 0) //skip empty blocks but always write first block to erase chip
                {
                    var report = PrepareReport(addr, dataBlock.ToArray(), BoardDef);
                    if (!device.WriteReport(report))  //if write fails (happens if teensy still busy) wait and retry once
                    {
                        Thread.Sleep(10);
                        if (!device.WriteReport(report)) return 2;
                    }
                    Thread.Sleep(addr == 0 ? 100 : 1); // First block needs more time since it erases the complete chip
                }

                addr += BoardDef.BlockSize;
            }

            // Reboot the device to start the downloaded firmware
            if (reboot)
            {
                var rebootReport = device.CreateReport();
                rebootReport.Data[0] = 0xFF;
                rebootReport.Data[1] = 0xFF;
                rebootReport.Data[2] = 0xFF;
                device.WriteReport(rebootReport);
            }

            return 0; // all good
        }

        public static byte[] GetEmptyFlashImage(PJRC_Board board)
        {
            int FlashSize = BoardDefinitions[board].FlashSize;
            return Enumerable.Repeat((byte)0xFF, FlashSize).ToArray();
        }

        public static bool StartHalfKay(uint Serialnumber)
        {
            using (var watcher = new TeensyWatcher())
            {
                var Teensy = watcher.ConnectedDevices.FirstOrDefault(d => d.Serialnumber == Serialnumber);
                if (Teensy == null) return false; // No device with given sn found
                if (Teensy.UsbType == USB_Device.USBtype.HalfKay) return true; // HalfKay already running on the device
                if (Teensy.UsbType != USB_Device.USBtype.UsbSerial) return false; // Unsupported USB mode

                //Start HalfKay                 
                using (var port = new SerialPort(Teensy.Port))
                {
                    port.Open();
                    port.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
                }
            }
            return true;
        }


        #endregion

        #region Private Methods and Fileds ----------------------------------------------------------------------

        private static HidReport PrepareReport(int addr, byte[] data, BoardDefinition BoardDef)
        {
            var report = new HidReport(BoardDef.BlockSize + BoardDef.DataOffset + 1);
            report.ReportId = 0;

            // Copy address bytes into report. Use function stored in board definition
            BoardDef.AddrCopy(report.Data, BitConverter.GetBytes(addr));

            // Copy datablock into report
            data.CopyTo(report.Data, BoardDef.DataOffset);

            return report;
        }

        static uint GetSerialNumber(HidDevice device)
        {
            byte[] sn;
            device.ReadSerialNumber(out sn);

            string snString = System.Text.Encoding.Unicode.GetString(sn).TrimEnd("\0".ToArray());

            var serialNumber = Convert.ToUInt32(snString, 16);
            if (serialNumber != 0xFFFFFFFF)
            {
                serialNumber *= 10;
            }
            return serialNumber;
        }

        /// <summary>
        /// Upload charactaristics for the PJRC boards as defined in Pauls "teensy_loader_cli.c" https://github.com/PaulStoffregen/teensy_loader_cli/blob/master/teensy_loader_cli.c
        /// The AddrCopy Action is used to copy the address bytes to the report         
        /// Currently only Teensy3.1 was tested, please inform if you find any errors on the other boards
        /// </summary>
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
            public int FlashSize;
            public int BlockSize;
            public int DataOffset;
            public string MCU;
            public Action<byte[], byte[]> AddrCopy;
        }

        #endregion
    }

    public enum PJRC_Board
    {
        Teensy_40,
        Teensy_36,
        Teensy_35,
        Teensy_31_2,
        Teensy_30,
        Teensy_LC,
        Teensy_2pp,
        Teensy_2,
        unknown,
    }
}
