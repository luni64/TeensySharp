using libTeensySharp;
using System;
using System.IO;
using System.Linq;
using static System.Console;

namespace Firmware_Upload_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = new TeensyWatcher();

            WriteLine("===============================================================");
            WriteLine(" Firmware Upload Tester");
            WriteLine("===============================================================\n");
            WriteLine("Found the following Teensies on the USB bus---------------------");

            foreach (var t in watcher.ConnectedTeensies)
            {
                WriteLine(t);
            }

            WriteLine("\nPlease press 'u' to program the first teensy in the list above, any other key to abort");
            if (ReadKey(true).Key != ConsoleKey.U) Environment.Exit(0);

            var teensy = watcher.ConnectedTeensies.FirstOrDefault();
            if (teensy != null)
            {
                var firmware = Path.Combine(Path.GetTempPath(), "MRKIV.hex"); 
                var result = teensy.Upload(firmware, reboot: true);
                WriteLine(result);
            }

            // cleanup            
            watcher.Dispose();

            WriteLine("\nPress any key to quit");
            while (!KeyAvailable) ;
        }
    }
}
