using System;
using System.IO;
using TeensySharp;
using System.Linq;
using System.Collections.Generic; 

namespace SimpleTestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Two test files 
            string file1 = "blink_slow.hex";
            string file2 = "blink_fast.hex";

            string testfile = file2;

            // Define the board to be programmed (all boards are implemented but currently only Teensy 3.1 is tested) 
            var Board = PJRC_Board.Teensy_31;

            // Obtain an empty flash image with the correct size and all bytes cleared (set to 0xFF)
            var FlashImage = SharpUploader.GetEmptyFlashImage(Board);

            using (var HexStream = File.OpenText(testfile))
            {
                // parse the file and write output to the image 
                SharpHexParser.ParseStream(HexStream, FlashImage);
                HexStream.Close();
            }

            using (var Watcher = new TeensyWatcher())
            {
                USB_Device Teensy = Watcher.ConnectedDevices.FirstOrDefault(); //We take the first Teensy we find...
                               
                Console.WriteLine("- Starting Bootloader for Teensy {0}...", Teensy.Serialnumber);
                bool res = SharpUploader.StartHalfKay(Teensy.Serialnumber);
                Console.WriteLine(res ? "  OK" : "  Bootloader not running");

                // Upload firmware image to the board and reboot
                Console.WriteLine("\n- Uploading {0}...", testfile);
                int result = SharpUploader.Upload(FlashImage, Board, Teensy.Serialnumber, reboot: true);
              
                // Show result
                switch (result)
                {
                    case 0:
                        Console.WriteLine("  Successfully uploaded");
                        break;
                    case 1:
                        Console.WriteLine("  Found no board with running HalfKay. Did you press the programming button?");
                        Console.WriteLine("  Aborting...");
                        break;
                    case 2:
                        Console.WriteLine("  Error during upload.");
                        Console.WriteLine("  Aborting...");
                        break;
                }
                Console.WriteLine("\nPress any key");
                while (!Console.KeyAvailable) ;
            }
        }
    }
}
