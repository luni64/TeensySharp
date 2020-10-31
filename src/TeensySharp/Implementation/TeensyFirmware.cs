using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static System.Globalization.CultureInfo;


namespace lunOptics.libTeensySharp.Implementation
{
    internal class TeensyFirmware : IFirmware
    {
        #region construction -----------------------------------------------------------
        internal TeensyFirmware(TextReader hexStream)
        {
            ParseStream(hexStream);
        }
        internal TeensyFirmware(String filename)
        {
            if (File.Exists(filename))
            {
                using (var stream = File.OpenText(filename))
                {
                    ParseStream(stream);
                }
            }
        }

        #endregion


        #region IFirmware implementation -----------------------------------------------

        public byte[] Getimage()
        {
            return image;
        }
               
        private byte[] image = null;

        private void Setimage(byte[] value)
        {
            image = value;
        }

        public PJRC_Board boardType { get; private set; } = PJRC_Board.unknown;
        #endregion

        private bool ParseStream(TextReader reader)
        {
            var records = new List<HexDataRecord>();

            int ULBA = 0;           // Upper Linear Base Address 
            int SEGBA = 0;          // Segment Base Address

            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                var record = ParseRecord(line, ref SEGBA, ref ULBA);
                if (record != null)
                {
                    records.Add(record);
                }
                else
                {
                    return false;
                }
            }
            var maxAddr = records.Where(d => d.type == RecordType.Data).Max(d => d.addr + d.data.Length);

            if (records[0].type == RecordType.ExLinAdr && records[0].data[0] == 0x60 && records[0].data[1] == 0x00) // check if we have a T4.0
            {
                Setimage(Enumerable.Repeat((byte)0xFF, maxAddr - 0x6000_0000).ToArray());
                foreach (var record in records.Where(r => r.type == RecordType.Data))
                {
                    record.data.CopyTo(Getimage(), record.addr - 0x6000_0000);
                }
                boardType = PJRC_Board.T4_0;
            }
            else
            {
                Setimage(Enumerable.Repeat((byte)0xFF, maxAddr).ToArray());
                foreach (var record in records.Where(r => r.type == RecordType.Data))
                {
                    record.data.CopyTo(Getimage(), record.addr);
                }
                boardType = IdentifyModel();
            }
            return true;
        }

        private PJRC_Board IdentifyModel()
        {
            const uint startup_size = 0x400;
            if (Getimage().Length >= startup_size)
            {
                UInt32 reset_handler_addr = BitConverter.ToUInt32(Getimage(), 4);
                if (reset_handler_addr >= startup_size) return PJRC_Board.unknown;

                UInt32 magic_check;
                PJRC_Board board;
                switch (reset_handler_addr)
                {
                    case 0xF9:
                        board = PJRC_Board.T3_0;
                        magic_check = 0x00043F82;
                        break;
                    case 0x1BD:
                        board = PJRC_Board.T3_2;
                        magic_check = 0x00043F82;
                        break;
                    case 0xC1:
                        board = PJRC_Board.T_LC;
                        magic_check = 0x00003F82;
                        break;
                    case 0x199:
                        board = PJRC_Board.T3_5;
                        magic_check = 0x00043F82;
                        break;
                    case 0x1D1:
                        board = PJRC_Board.T3_6;
                        magic_check = 0x00043F82;
                        break;
                    default:
                        board = PJRC_Board.unknown; // ToDo: Need to find the correct bytes for the T4.0
                        magic_check = 0;
                        break;
                }

                for (int offs = (int)reset_handler_addr; offs < startup_size - 4; offs++)
                {
                    UInt32 value4 = BitConverter.ToUInt32(Getimage(), offs);
                    if (value4 == magic_check)
                    {
                        return board;
                    }
                }
            }
            return PJRC_Board.unknown;
        }

        private static HexDataRecord ParseRecord(string line, ref int SEGBA, ref int ULBA)
        {
            var record = new HexDataRecord();

            if (line.Length >= 11 && line[0] == ':')
            {
                byte RecLen = Convert.ToByte(line.Substring(1, 2), 16);
                int DRLO = Convert.ToUInt16(line.Substring(3, 4), 16);  // Data Record Load Offset
                record.type = (RecordType)Convert.ToByte(line.Substring(7, 2), InvariantCulture);

                if (line.Length == 11 + 2 * RecLen)
                {
                    record.data = new byte[RecLen];
                    for (int i = 0; i < RecLen; i++)
                    {
                        record.data[i] = Convert.ToByte(line.Substring(9 + i * 2, 2), 16);
                    }

                    switch (record.type)
                    {
                        case RecordType.Data:
                            record.addr = ULBA + SEGBA + DRLO;
                            break;

                        case RecordType.ExLinAdr:
                            SEGBA = 0;
                            ULBA = (256 * record.data[0] + record.data[1]) << 16;
                            // if (ULBA == 0x6000_0000) ULBA = 0;  // HACK, need to clarify why the flash is stored at 0x6000_0000 for T4.0
                            break;

                        case RecordType.ExSegAdr:
                            ULBA = 0;
                            SEGBA = (256 * record.data[0] + record.data[1]) << 4;
                            break;

                            //todo: Add code for RecordType.EOF
                    }

                    //todo: Add code for checksum evaluation
                    return record;
                }
            }
            return null;
        }

        private enum RecordType
        {
            Data = 0x00,
            EOF = 0x01,
            ExSegAdr = 0x02,
            StartSegAdr = 0x03,
            ExLinAdr = 0x04,
            StartLinAdr = 0x05
        };

        private class HexDataRecord
        {
            public RecordType type;
            public int addr;
            public byte[] data;
        }
    }
}

