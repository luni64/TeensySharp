using System;
using System.IO.Ports;
using System.Linq;
using lunOptics.TeensySharp;

namespace TeensyWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var Watcher = new TeensyWatcher();
            TeensySharpLib.ConnectedBoardsChanged += ConnectedTeensiesChanged; //Connect an eventhandler to get information about changes (optional) 

            // Display currently connected Teensies
            Console.WriteLine("Currently the following Teensies are connected:");
            foreach (var Teensy in TeensySharpLib.ConnectedBoards)
            {
                if (Teensy.UsbType == UsbType.Serial)
                {
                    Console.WriteLine("USBSerial: Serialnumber {0}, on {1}", Teensy.Serialnumber, Teensy.Port);
                }
                else Console.WriteLine("HalfKay: Serialnumber {0}", Teensy.Serialnumber);
            }

            // Here is a good place to construct a SerialPort Object
            // For the sake of simplicity lets take the first one from the list
            var myTeensy = TeensySharpLib.ConnectedBoards.FirstOrDefault();
            if (myTeensy != null && myTeensy.UsbType == UsbType.Serial)
            {
                using (var Com = new SerialPort(myTeensy.Port))
                {
                    // lets talk to our Teensy: 
                    Com.Open();
                    Com.WriteLine("Hello Teensy");
                    Com.Close();
                }
            }

            Console.WriteLine("\nPlug Teensies in / out to see the watcher in action\n\nPress any key to exit");
            while (!Console.KeyAvailable) ;

            // CleanUp 
            TeensySharpLib.ConnectedBoardsChanged -= ConnectedTeensiesChanged;
        }

        //-------------------------------------------------------------------------------------------------------------------
        //In a real application you would use this eventhandler to inform the user, connect / disconnect the SerialPort etc..
        //
        static void ConnectedTeensiesChanged(object sender, ConnectedBoardsChangedEventArgs e)
        {
            // Write information about the added or removed Teensy to the console         

            ITeensy Teensy = e.changedDevice;

            switch (Teensy.UsbType)
            {
                case UsbType.HalfKay:
                    if (e.changeType == ChangeType.add)
                    {
                        Console.WriteLine("Teensy {0} connected (running HalfKay)", Teensy.Serialnumber);
                    }
                    else
                    {
                        Console.WriteLine("Teensy {0} removed", Teensy.Serialnumber);
                    }
                    break;

                case UsbType.Serial:
                    if (e.changeType == ChangeType.add)
                    {
                        Console.WriteLine("Teensy {0} connected on {1}", Teensy.Serialnumber, Teensy.Port);
                    }
                    else
                    {
                        Console.WriteLine("Teensy {0} removed from {1}", Teensy.Serialnumber, Teensy.Port);
                    }
                    break;
            }
        }
    }
}
