using System;
using System.Collections.Generic;
using System.Text;

namespace lunOptics.TeensySharp
{
    public interface IFirmware
    {
        byte[] image { get; }
        PJRC_Board boardType { get; }
    }
}
