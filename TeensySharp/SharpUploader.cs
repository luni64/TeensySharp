using HidLibrary;
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


        static HidDevice GetDeviceFromSerialnumber(string Serialnumber)
        {
            var devices = HidDevices.Enumerate(0x16C0, 0x0478);
            var device = Serialnumber == "" ? devices.FirstOrDefault() : devices.FirstOrDefault(x => GetSerialNumber(x) == Serialnumber);

            if (device == null)
            {
                StartHalfKay(Serialnumber);
                DateTime start = DateTime.Now;
                while (DateTime.Now - start < TimeSpan.FromSeconds(5))
                {
                    devices = HidDevices.Enumerate(0x16C0, 0x0478);
                //    var device = Serialnumber == "" ? devices.FirstOrDefault() : devices.FirstOrDefault(x => GetSerialNumber(x) == Serialnumber);

                }
            }

            return device;
        }

        public static int Upload(byte[] Image, PJRC_Board board, string Serialnumber = "", bool reboot = true)
        {
            var BoardDef = BoardDefinitions[board];

            var devices = HidDevices.Enumerate(0x16C0, 0x0478);
            var device = Serialnumber == "" ? devices.FirstOrDefault() : devices.FirstOrDefault(x => GetSerialNumber(x) == Serialnumber);

            if (device == null)
            {
                StartHalfKay(Serialnumber);
            }

            int addr = 0;
            foreach (var dataBlock in Image.Batch(BoardDef.BlockSize))
            {
                if (dataBlock.Any(d => d != 0xFF) || addr == 0) //skip empty blocks but always write first block to erase chip
                {
                    var report = PrepareReport(addr, dataBlock.ToArray(), BoardDef);
                    if (!device.WriteReport(report))  //if first write fails (happens) wait and retry once
                    {
                        Thread.Sleep(10);
                        if (!device.WriteReport(report)) return 2;
                    }
                    Thread.Sleep(addr == 0 ? 100 : 1); // First block needs more time since it erases the complete chip
                }
                //Console.WriteLine(addr.ToString() + "-" + (addr + BoardDef.BlockSize).ToString() + " OK");

                addr += BoardDef.BlockSize;
            }

            Thread.Sleep(5);

            if (reboot)
            {
                var rebootReport = device.CreateReport();
                rebootReport.Data[0] = 0xFF;
                rebootReport.Data[1] = 0xFF;
                rebootReport.Data[2] = 0xFF;
                device.WriteReport(rebootReport);
            }

            return 0;
        }

        public static byte[] GetEmptyFlashImage(PJRC_Board board)
        {
            int FlashSize = BoardDefinitions[board].FlashSize;
            return Enumerable.Repeat((byte)0xFF, FlashSize).ToArray();
        }

        public static bool StartHalfKay(string Serialnumber)
        {
            //using (var watcher = new TeensyWatcher())
            //{
            //    var Teensy = watcher.ConnectedDevices.FirstOrDefault(d => d.Serialnumber == Serialnumber);
            //    if (Teensy == null) return false;

            //    using (var port = new SerialPort(Teensy.Port))
            //    {
            //        port.Open();
            //        int previousBR = port.BaudRate;
            //        port.BaudRate = 134;
            //        port.BaudRate = previousBR;
            //        port.Close();
            //    }
            //}
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

        static string GetSerialNumber(HidDevice device)
        {
            byte[] sn;
            device.ReadSerialNumber(out sn);

            string snString = System.Text.Encoding.Unicode.GetString(sn).TrimEnd("\0".ToArray());
            return (Convert.ToUInt32(snString, 16) * 10).ToString();
        }

        /// <summary>
        /// Upload charactaristics for the PJRC boards as defined in Pauls "teensy_loader_cli.c" https://github.com/PaulStoffregen/teensy_loader_cli/blob/master/teensy_loader_cli.c
        /// The AddrCopy Action is used to copy the address bytes to the report         
        /// Currently only Teensy3.1 was tested, please inform if you find any errors on the other boards
        /// </summary>
        private static Dictionary<PJRC_Board, BoardDefinition> BoardDefinitions = new Dictionary<PJRC_Board, BoardDefinition>()
        {    
            {PJRC_Board.Teensy_31, new BoardDefinition 
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
        Teensy_LC,
        Teensy_31,
        Teensy_30,
        Teensy_2pp,
        Teensy_2
    }
}
