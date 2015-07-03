using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace TeensySharp
{
    // Just specializing the UsbWatcher Class to the PJRC VID and the Teensy Serial PID
    public class TeensyWatcher : UsbWatcher
    {
        public TeensyWatcher()
            : base("16C0", "0483") 
        { }
    }


    public class UsbWatcher : IDisposable
    {
        #region Properties and Events -----------------------------------------------

        public List<USB_Serial_Device> ConnectedDevices { get; private set; }
        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        #endregion

        #region Construction / Destruction ------------------------------------------

        public UsbWatcher(string vid, string pid)
        {
            PORT_HWID = @"USB\VID_" + vid + "&PID_" + pid;

            ConnectedDevices = new List<USB_Serial_Device>();

            // look for already connected boards and add them to list


            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_USBControllerDevice"))
            {
                string res = "";

                foreach (var mgmtObject in searcher.Get())
                {
                    var s = mgmtObject.Properties;
                    res += mgmtObject["Dependent"] + "\n";
                    res += mgmtObject["Antecedent"] + "\n"; 
                }
            }


            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_SerialPort"))
            {
                foreach (var mgmtObject in searcher.Get())
                {
                    var device = MakeDevice(mgmtObject, PORT_HWID);
                    if (device != null)
                    {
                        ConnectedDevices.Add(device);
                    }
                }
            }

            StartWatching();
        }

        public void Dispose()
        {
            Console.WriteLine("---------DISPOSE UsbSerialPortWatcher -----------");
            StopWatching();
        }

        #endregion

        #region Port Watching  ------------------------------------------------------

        protected ManagementEventWatcher CreateWatcher = null;
        protected ManagementEventWatcher DeleteWatcher = null;

        protected void StartWatching()
        {
            StopWatching();

            DeleteWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceDeletionEvent",
                    Condition = "TargetInstance ISA 'Win32_SerialPort'",
                    WithinInterval = new TimeSpan(0, 0, 1),
                },
            };
            DeleteWatcher.EventArrived += PortsChanged;
            DeleteWatcher.Start();

            CreateWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceCreationEvent",
                    Condition = "TargetInstance ISA 'Win32_SerialPort'",
                    WithinInterval = new TimeSpan(0, 0, 1),
                },
            };
            CreateWatcher.EventArrived += PortsChanged;
            CreateWatcher.Start();
        }

        protected void StopWatching()
        {
            if (CreateWatcher != null)
            {
                CreateWatcher.Stop();
                CreateWatcher.Dispose();
            }
            if (DeleteWatcher != null)
            {
                DeleteWatcher.Stop();
                DeleteWatcher.Dispose();
            }
        }

        private string PORT_HWID;

        public enum ChangeType
        {
            add,
            remove
        }
        
        void PortsChanged(object sender, EventArrivedEventArgs e)
        {
            var device = MakeDevice((ManagementBaseObject)e.NewEvent["TargetInstance"], PORT_HWID);
            if (device != null)
            {
                ChangeType type = e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent" ? ChangeType.add : ChangeType.remove;

                if (type == ChangeType.add)
                {
                    ConnectedDevices.Add(device);
                    OnConnectionChanged(type, device);
                }
                else
                {
                    var rd = ConnectedDevices.Find(d => d.Serialnumber == device.Serialnumber);
                    ConnectedDevices.Remove(rd);
                    OnConnectionChanged(type, rd);
                }
            }
        }
        
        #endregion

        #region Helpers

        protected USB_Serial_Device MakeDevice(ManagementBaseObject mgmtObj, string ID)
        {
            var PnPDeviceID = (string)mgmtObj["PNPDeviceID"];

            if (PnPDeviceID.StartsWith(ID))
            {
                string port = (string)mgmtObj["DeviceID"];
                var SN = (PnPDeviceID.Split(new string[] { "\\" }, StringSplitOptions.None)).Last();
                return new USB_Serial_Device { Port = port, Serialnumber = SN };
            }
            return null;
        }

        #endregion

        #region EventHandler --------------------------------------------------------
        
        protected void OnConnectionChanged(ChangeType type, USB_Serial_Device changedDevice)
        {
            if (ConnectionChanged != null) ConnectionChanged(this, new ConnectionChangedEventArgs(type, changedDevice));
        }

        #endregion
    }

    public class USB_Serial_Device
    {
        public string Serialnumber { get; set; }
        public string Port { get; set; }
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public readonly UsbWatcher.ChangeType changeType;
        public readonly USB_Serial_Device changedDevice;

        public ConnectionChangedEventArgs(UsbWatcher.ChangeType type, USB_Serial_Device changedDevice)
        {
            this.changeType = type;
            this.changedDevice = changedDevice;
        }
    }
}



