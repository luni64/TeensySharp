using System;
using System.Linq;
using System.IO;

namespace TeensySharp
{
    public static class SharpHexParser
    {
        static uint ULBA;  // Upper Linear Base Address 
        static uint SEGBA; // Segment Base Address

        static public bool ParseStream(TextReader r, byte[] Image)
        {
            ULBA = 0;
            SEGBA = 0;

            string line = r.ReadLine();
            while (line != null)
            {
                if (!ParseLine(line, Image)) return false;
                line = r.ReadLine();
            }
            return true;
        }

        static private bool ParseLine(string line, byte[] FW_Image)
        {
            if (line.Length >= 11 && line[0] == ':')
            {
                byte RecLen = Convert.ToByte(line.Substring(1, 2), 16);
                uint DRLO = Convert.ToUInt16(line.Substring(3, 4), 16);  // Data Record Load Offset
                var RECTYP = (RecordType)Convert.ToByte(line.Substring(7, 2));

                if (line.Length == 11 + 2 * RecLen)
                {
                    var data = new byte[RecLen]; 
                    for (int i = 0; i < RecLen; i++)
                    {
                        data[i] = Convert.ToByte(line.Substring(9 + i * 2, 2), 16);
                    }

                    switch (RECTYP)
                    {
                        case RecordType.Data:
                            uint StartAdr = ULBA + SEGBA + DRLO;
                            data.CopyTo(FW_Image, StartAdr);
                            break;

                        case RecordType.ExLinAdr:
                            SEGBA = 0;                            
                            ULBA = (256U * data[0] + data[1]) << 16;
                            break;

                        case RecordType.ExSegAdr:
                            ULBA = 0;
                            SEGBA = (256U * data[0] + data[1]) << 4;
                            break;

                        //todo: Add code for RecordType.EOF
                    }

                    //todo: Add code for checksum evaluation
                    return true;
                }
            }
            return false;
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
    }
}
