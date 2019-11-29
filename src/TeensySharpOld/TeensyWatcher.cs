using HidLibrary;
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
        string vidStr = "'%USB_VID[_]" + vid.ToString("X") + "%'";

        #region Properties and Events -----------------------------------------------

        public List<USB_Device> ConnectedDevices { get; } = new List<USB_Device>();
        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        #endregion

        #region Construction / Destruction ------------------------------------------

        public TeensyWatcher()
        {
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

                PJRC_Board board;
                var hwid = ((string[])mgmtObj["HardwareID"])[0];
                switch (hwid.Substring(hwid.IndexOf("REV_") + 4, 4))
                {
                    case "0273":
                        board = PJRC_Board.Teensy_LC;
                        break;
                    case "0274":
                        board = PJRC_Board.Teensy_30;
                        break;
                    case "0275":
                        board = PJRC_Board.Teensy_31_2;
                        break;
                    case "0276":
                        board = PJRC_Board.Teensy_35;
                        break;
                    case "0277":
                        board = PJRC_Board.Teensy_36;
                        break;
                    case "0279":
                        board = PJRC_Board.Teensy_40;
                        break;
                    default:
                        board = PJRC_Board.unknown;
                        break;
                }

                return new USB_Device
                {
#pragma warning disable 0618 // obsolete warning
                    Type = USB_Device.USBtype.UsbSerial,
                    UsbType = USB_Device.USBtype.UsbSerial,
                    Port = port,
                    Serialnumber = serNum,
                    Board = board,
                    BoardType = board
#pragma warning restore 0618
                };
            }
            else if (pid == halfKayPid)
            {
                // getting the hid device like done in the following block is not very efficient
                // need to find a way to extract the device path from the WMI and construct
                // the devcie using HidDevices.GetDevice
                var hwid = ((string[])mgmtObj["HardwareID"])[0];
                uint serNum = Convert.ToUInt32(DeviceIdParts[2], 16);
                if (serNum != 0xFFFFFFFF) // diy boards without serial number
                {
                    serNum *= 10;
                }

                var devices = HidDevices.Enumerate((int)vid, (int)halfKayPid); // Get all boards with running HalfKay
                var device = devices.FirstOrDefault(x => GetSerialNumber(x) == serNum);

                PJRC_Board board = PJRC_Board.unknown;

                switch (device?.Capabilities.Usage)
                {
                    case 0x1A: board = PJRC_Board.unknown; break;
                    case 0x1B: board = PJRC_Board.Teensy_2; break;
                    case 0x1C: board = PJRC_Board.Teensy_2pp; break;
                    case 0x1D: board = PJRC_Board.Teensy_30; break;
                    case 0x1E: board = PJRC_Board.Teensy_31_2; break;
                    case 0x20: board = PJRC_Board.Teensy_LC; break;
                    case 0x21: board = PJRC_Board.Teensy_31_2; break;
                    case 0x1F: board = PJRC_Board.Teensy_35; break;
                    case 0x22: board = PJRC_Board.Teensy_36; break;
                    case 0x24: board = PJRC_Board.Teensy_40; break;
                    default: board = PJRC_Board.unknown; break;
                }

                return new USB_Device
                {
#pragma warning disable 0618
                    Type = USB_Device.USBtype.HalfKay,
                    UsbType = USB_Device.USBtype.HalfKay,
                    Port = "",
                    Serialnumber = serNum,
                    Board = board,
                    BoardType = board
#pragma warning restore 0618
                };
            }
            else return null;
        }

        static uint GetSerialNumber(HidDevice device)
        {
            byte[] sn;
            device.ReadSerialNumber(out sn);

            string snString = System.Text.Encoding.Unicode.GetString(sn).TrimEnd("\0".ToArray());

            var serialNumber = Convert.ToUInt32(snString, 16);
            if (serialNumber != 0xFFFFFFFF)
            {
                serialNumber *= 10;
            }
            return serialNumber;
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
        public enum USBtype
        {
            UsbSerial,
            HalfKay,
            HID,
            //...
        }

        [Obsolete("Please use UsbType instead.", false)]
        public USBtype Type;
        public USBtype UsbType;
        public uint Serialnumber { get; set; }
        public string Port { get; set; }
        [Obsolete("Please use BoardType instead.", false)]
        public PJRC_Board Board { get; set; }
        public PJRC_Board BoardType;
        public string BoardId
        {
            get
            {
                switch (BoardType)
                {
                    case PJRC_Board.Teensy_2: return $"Teensy 2 ({Serialnumber})";
                    case PJRC_Board.Teensy_2pp: return $"Teensy 2++ ({Serialnumber})";
                    case PJRC_Board.Teensy_LC: return $"Teensy LC ({Serialnumber})";
                    case PJRC_Board.Teensy_30: return $"Teensy 3.0 ({Serialnumber})";
                    case PJRC_Board.Teensy_31_2: return $"Teensy 3.2 ({Serialnumber})";
                    case PJRC_Board.Teensy_35: return $"Teensy 3.5 ({Serialnumber})";
                    case PJRC_Board.Teensy_36: return $"Teensy 3.6 ({Serialnumber})";
                    case PJRC_Board.Teensy_40: return $"Teensy 4.0 ({Serialnumber})";
                    case PJRC_Board.unknown: return $"Unknown Board ({Serialnumber})";
                    default: return null;
                }
            }
        }
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





