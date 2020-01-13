using BetterWin32Errors;
using lunOptics.libTeensySharp;
using lunOptics.libUsbTree;
using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using static lunOptics.libTeensySharp.Native.NativeWrapper;

namespace libTeensySharp.Implementation
{
    internal static class TTeensy
    {
        public async static Task<bool> ResetAsync(UsbTeensy teensy, double timeout = 5.0)
        {
            if (teensy.UsbType != UsbType.HalfKay)
            {
                if (await RebootAsync(teensy, timeout) == false) return false;
            }

            using (var hidHandle = getFileHandle(teensy.interfaces[0].DeviceInstanceID))
            {
                if (hidHandle?.IsInvalid ?? true)
                {
                    hidHandle?.Dispose();
                    return false;
                }


            }



            return false;
        }

        public async static Task<bool> RebootAsync(UsbTeensy teensy, double timeout = 5.0)
        {
            try
            {
                if (teensy.UsbType == UsbType.HalfKay) return await Task.FromResult(true);  // already rebooted  
                if (teensy.UsbType == UsbType.Serial) rebootSerial(teensy.Port);            // simple serial device, can be rebooted directly
                else                                                                        // composite device, check for a rebootable interface
                {
                    foreach (UsbTeensy f in teensy.functions)                               // do we have a rebootable function (serial or seremu)?
                    {
                        if (f.UsbType == UsbType.Serial)  // serial function
                        {
                            rebootSerial(f.Port);
                            break;
                        }
                        else                              // seremu function
                        {
                            var iface = f.interfaces.FirstOrDefault(i => i.HidUsageID == UsbTeensy.SerEmuUsageID);
                            if (iface != null)
                            {
                                rebootSerEmu(iface);
                                break;
                            }
                        }
                    }
                }

                // wait until teensy appears as bootloader 
                await Task.Run(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    while (teensy.UsbType != UsbType.HalfKay && sw.Elapsed < TimeSpan.FromSeconds(timeout))
                    {
                        Task.Delay(10).Wait();
                    }
                });

                return teensy.UsbType == UsbType.HalfKay;
            }
            catch (Win32Exception)
            {
                return false;
            }
        }

        private static void rebootSerial(string portName)
        {
            using (var p = new SerialPort(portName))
            {
                p.Open();
                p.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
            }
        }

        private static bool rebootSerEmu(UsbDevice iface)
        {
            var hidHandle = getFileHandle(iface.DeviceInstanceID);
            if (hidHandle?.IsInvalid ?? true)
            {
                hidHandle?.Dispose();
                return false;
            }

            bool result = hidWriteFeature(hidHandle, new byte[] { 0x00, 0xA9, 0x45, 0xC2, 0x6B });
            hidHandle.Dispose();
            return result;
        }
    }
}
