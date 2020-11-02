using lunOptics.libUsbTree;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lunOptics.libTeensySharp
{
    public interface ITeensy : IUsbDevice
    {
        int Serialnumber { get; }
        UsbType UsbType { get; }

        List<string> Ports { get; }
        PJRC_Board BoardType { get; }

        ErrorCode Reboot(TimeSpan? timeout = null);
        ErrorCode Reset(TimeSpan? timeout = null);
        ErrorCode Upload(string hexFile, bool reboot = true, TimeSpan? timeout = null);
        Task<ErrorCode> ResetAsync(TimeSpan? timeout = null);      
        Task<ErrorCode> RebootAsync(TimeSpan? timeout = null);        
        Task<ErrorCode> UploadAsync(string hexFile, IProgress<int> progress = null, bool reboot = true, TimeSpan? timeout = null);
    }
}
