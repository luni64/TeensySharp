using System;
using System.IO.Ports;
using System.Linq;
using TeensySharp;

namespace TeensyWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var Watcher = new TeensyWatcher();
            Watcher.ConnectionChanged += ConnectedTeensiesChanged; //Connect an eventhandler to get information about changes (optional) 

            // Display currently connected Teensies
            Console.WriteLine("Currently the following Teensies are connected:");
            foreach (var Teensy in Watcher.ConnectedDevices)
            {
                if (Teensy.UsbType == USB_Device.USBtype.UsbSerial)
                {
                    Console.WriteLine("USBSerial: Serialnumber {0}, on {1}", Teensy.Serialnumber, Teensy.Port);
                }
                else Console.WriteLine("HalfKay: Serialnumber {0}", Teensy.Serialnumber);
            }
            
            // Here is a good place to construct a SerialPort Object
            // For the sake of simplicity lets take the first one from the list
            var myTeensy = Watcher.ConnectedDevices.FirstOrDefault();
            if (myTeensy != null && myTeensy.UsbType == USB_Device.USBtype.UsbSerial && myTeensy.Port != null)
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
            Watcher.ConnectionChanged -= ConnectedTeensiesChanged;
            Watcher.Dispose();
        }

        //-------------------------------------------------------------------------------------------------------------------
        //In a real application you would use this eventhandler to inform the user, connect / disconnect the SerialPort etc..
        //
        static void ConnectedTeensiesChanged(object sender, ConnectionChangedEventArgs e)
        {
            // Write information about the added or removed Teensy to the console         

            USB_Device Teensy = e.changedDevice;

            switch (Teensy.UsbType)
            {
                case USB_Device.USBtype.HalfKay:
                    if (e.changeType == TeensyWatcher.ChangeType.add)
                    {
                        Console.WriteLine("Teensy {0} running HalfKay", Teensy.Serialnumber);
                    }
                    else
                    {
                        Console.WriteLine("Teensy {0} removed", Teensy.Serialnumber);
                    }
                    break;

                case USB_Device.USBtype.UsbSerial:
                    if (e.changeType == TeensyWatcher.ChangeType.add)
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
