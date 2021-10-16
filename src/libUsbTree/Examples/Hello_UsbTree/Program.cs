using lunOptics.libUsbTree;
using System;

namespace Hello_UsbTree
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var usbTree = new UsbTree())
            {
                foreach (var device in usbTree.DeviceList)
                {
                    Console.WriteLine(device);
                }
            }

            Console.WriteLine("\n\nAny key to exit");
            while (!Console.KeyAvailable) ;
        }
    }
}
