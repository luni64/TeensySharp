using System;
using System.IO;
using TeensySharp.Implementation;

namespace TeensySharp.Interface
{
    public class Firmware
    {
        static public IFirmware Parse(TextReader hexStream)
        {
            return new TeensyFirmware(hexStream);
        }
        static public IFirmware Parse(String filename)
        {
            return new TeensyFirmware(filename);
        }
    }
}
