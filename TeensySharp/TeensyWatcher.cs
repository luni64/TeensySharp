using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace TeensySharp
{
    public class TeensyWatcher : IDisposable
    {
        const uint vid = 0x16C0;
        const uint serPid = 0x483;
        const uint halfKayPid = 0x478;
        const string vidStr = "'%USB_VID[_]16C0%'";

        #region Properties and Events -----------------------------------------------

        public List<USB_Device> ConnectedDevices { get; private set; }
        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        #endregion

        #region Construction / Destruction ------------------------------------------

        public TeensyWatcher()
        {
            ConnectedDevices = new List<USB_Device>();

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE " + vidStr))
            {
                foreach (var mgmtObject in searcher.Get())
                {
                    var device = MakeDevice(mgmtObject);
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
            StopWatching();
        }

        #endregion

        #region Port Watching  ------------------------------------------------------

        protected ManagementEventWatcher CreateWatcher = null;
        protected ManagementEventWatcher DeleteWatcher = null;

        protected void StartWatching()
        {
            StopWatching(); // Just to make sure 

            DeleteWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceDeletionEvent",
                    Condition = "TargetInstance ISA 'Win32_PnPEntity'",
                    WithinInterval = new TimeSpan(0, 0, 1), //Todo: make the interval settable
                },
            };
            DeleteWatcher.EventArrived += PortsChanged;
            DeleteWatcher.Start();

            CreateWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceCreationEvent",
                    Condition = "TargetInstance ISA 'Win32_PnPEntity'",
                    WithinInterval = new TimeSpan(0, 0, 1), //Todo: make the interval settable
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

        public enum ChangeType
        {
            add,
            remove
        }

        void PortsChanged(object sender, EventArrivedEventArgs e)
        {
            var device = MakeDevice((ManagementBaseObject)e.NewEvent["TargetInstance"]);
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

        protected USB_Device MakeDevice(ManagementBaseObject mgmtObj)
        {
            var DeviceIdParts = ((string)mgmtObj["PNPDeviceID"]).Split("\\".ToArray());

            if (DeviceIdParts[0] != "USB") return null;

            int start = DeviceIdParts[1].IndexOf("PID_") + 4;
            uint pid = Convert.ToUInt32(DeviceIdParts[1].Substring(start, 4), 16);

            if (pid == serPid)
            {
                uint serNum = Convert.ToUInt32(DeviceIdParts[2]);
                string port = (((string)mgmtObj["Caption"]).Split("()".ToArray()))[1];

                return new USB_Device
                {
                    Type = USB_Device.type.UsbSerial,
                    Port = port,
                    Serialnumber = serNum
                };
            }
            else if (pid == halfKayPid)
            {
                uint serNum = Convert.ToUInt32(DeviceIdParts[2], 16) * 10;

                return new USB_Device
                {
                    Type = USB_Device.type.HalfKay,
                    Port = "",
                    Serialnumber = serNum,
                };
            }
            return null;
        }

        #endregion

        #region EventHandler --------------------------------------------------------

        protected void OnConnectionChanged(ChangeType type, USB_Device changedDevice)
        {
            if (ConnectionChanged != null) ConnectionChanged(this, new ConnectionChangedEventArgs(type, changedDevice));
        }

        #endregion
    }

    public class USB_Device
    {
        public enum type
        {
            UsbSerial,
            HalfKay, 
            HID,
            //...
        }

        public type Type;
        public uint Serialnumber { get; set; }
        public string Port { get; set; }
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public readonly TeensyWatcher.ChangeType changeType;
        public readonly USB_Device changedDevice;

        public ConnectionChangedEventArgs(TeensyWatcher.ChangeType type, USB_Device changedDevice)
        {
            this.changeType = type;
            this.changedDevice = changedDevice;
        }
    }
}



