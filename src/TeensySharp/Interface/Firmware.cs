using System;
using System.IO;
using lunOptics.TeensySharp.Implementation;

namespace lunOptics.TeensySharp
{
    public static class Firmware
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
