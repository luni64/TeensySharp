using System;
using System.IO;
using lunOptics.libTeensySharp.Implementation;

namespace lunOptics.libTeensySharp
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
