using System;
using System.Collections.Generic;
using System.Text;

namespace TeensySharp.Interface
{
    public interface IFirmware
    {
        byte[] image { get; }
        PJRC_Board boardType { get; }
    }
}
