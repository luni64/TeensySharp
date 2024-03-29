﻿using lunOptics.libUsbTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lunOptics.libTeensySharp
{
    public interface ITeensy : IUsbDevice
    {
        uint Serialnumber { get; }
        UsbType UsbType { get; }

        List<string> Ports { get; }
        PJRC_Board BoardType { get; }

        Task<ErrorCode> ResetAsync(TimeSpan? timeout = null);      
        Task<ErrorCode> RebootAsync(TimeSpan? timeout = null);        
        Task<ErrorCode> UploadAsync(string hexFile, IProgress<int> progress = null, bool reboot = true, TimeSpan? timeout = null);
        
        ErrorCode CheckPort();
    }
}
