using libTeensySharp.Native;
using lunOptics.libTeensySharp;
using lunOptics.LibUsbTree;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace libTeensySharp.Implementation
{
    internal static class TTeensy
    {
        public static async Task<bool> Reboot(UsbTeensy teensy)
        {
            HidHelper.test();

            switch (teensy.UsbType)
            {
                case UsbType.HalfKay: 
                    return true;

                case UsbType.Serial:
                    using (var port = new SerialPort(teensy.Port))
                    {
                        port.Open();
                        port.BaudRate = 134; //This will switch the board to HalfKay. Don't try to access port after this...                   
                    }
                    break;

                case UsbType.HID:
                    {
                     
                    }
                    break;

            }

            while (teensy.IsConnected)
            {
                await Task.Delay(10);                
            }
            Debug.WriteLine("rebooted");

            return true;
        }


    }
}
