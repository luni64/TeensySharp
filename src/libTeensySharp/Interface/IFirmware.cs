using System;
using System.Collections.Generic;
using System.Text;

namespace lunOptics.libTeensySharp
{
    public interface IFirmware
    {
        byte[] Getimage();

        PJRC_Board boardType { get; }
    }
}
