using HidLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using TeensySharp.Interface;


namespace TeensySharp
{
    public class TeensyWatcher : IDisposable
    {
        const uint vid = 0x16C0;
        const uint serPid = 0x483;
        const uint halfKayPid = 0x478;
        static string vidStr = "'%USB_VID[_]" + vid.ToString("X") + "%'";

        #region Properties and Events -----------------------------------------------

        public List<ITeensy> ConnectedDevices { get; } = new List<ITeensy>();
        public event EventHandler<ConnectionChangedEventArgs> ConnectionChanged;

        #endregion

        #region Construction / Destruction ------------------------------------------
        public static List<ITeensy> getConnectedTeensies()
        {
            List<ITeensy> result = new List<ITeensy>();

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE " + vidStr))
            {
                foreach (var mgmtObject in searcher.Get())
                {
                    var device = MakeDevice(mgmtObject);
                    if (device != null)
                    {
                        result.Add(device);
                    }
                }
            }
            return result;
        }

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
                    var rd = ConnectedDevices.Find(d => d.serialnumber == device.serialnumber);
                    ConnectedDevices.Remove(rd);
                    OnConnectionChanged(type, rd);
                }
            }
        }

        #endregion

        #region Helpers

        internal static Teensy_t MakeDevice(ManagementBaseObject mgmtObj)
        {
            var DeviceIdParts = ((string)mgmtObj["PNPDeviceID"]).Split("\\".ToArray());
            if (DeviceIdParts[0] != "USB") return null;

            var vidPidMi = DeviceIdParts[1].Split("&".ToArray());
            if (vidPidMi.Length != 2) return null;  // we are only interested in devices, not interfaces

            PJRC_Board board;
            uint pid = Convert.ToUInt32(vidPidMi[1].Substring(4, 4), 16);
            if (pid == halfKayPid)
            {
                string s = DeviceIdParts[2];
                uint serNum = Convert.ToUInt32(DeviceIdParts[2], 16);
                if (serNum != 0xFFFFFFFF) { serNum *= 10; }// diy boards without serial number

                var hidDev = HidDevices.Enumerate((int)vid, (int)halfKayPid).FirstOrDefault(x => GetSerialNumber(x, 16) == serNum);
                switch (hidDev?.Capabilities.Usage)
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

                return new Teensy_t
                {
                    usbType = USBtype.HalfKay,
                    port = "",
                    serialnumber = serNum,
                    boardType = board,
                    hidDevice = hidDev,
                };
            }

            else // Serial or HID
            {
                uint serNum = Convert.ToUInt32(DeviceIdParts[2]);  // these devices code the S/N as decimal number

                var hwid = ((string[])mgmtObj["HardwareID"])[0];
                switch (hwid.Substring(hwid.IndexOf("REV_") + 4, 4))
                {
                    case "0273": board = PJRC_Board.Teensy_LC; break;
                    case "0274": board = PJRC_Board.Teensy_30; break;
                    case "0275": board = PJRC_Board.Teensy_31_2; break;
                    case "0276": board = PJRC_Board.Teensy_35; break;
                    case "0277": board = PJRC_Board.Teensy_36; break;
                    case "0279": board = PJRC_Board.Teensy_40; break;
                    default: board = PJRC_Board.unknown; break;
                }

                return new Teensy_t
                {
                    usbType = pid == serPid ? USBtype.UsbSerial : USBtype.HID,
                    port = pid == serPid ? (((string)mgmtObj["Caption"]).Split("()".ToArray()))[1] : "",
                    serialnumber = serNum,
                    boardType = board,
                    hidDevice = pid == serPid ? null : HidDevices.Enumerate((int)vid, (int)pid).FirstOrDefault(x => GetSerialNumber(x, 10) == serNum)
                };
            }
        }

        static internal uint GetSerialNumber(HidDevice hidDevice, int Base)
        {
            hidDevice.ReadSerialNumber(out byte[] sn);
            string snString = System.Text.Encoding.Unicode.GetString(sn).TrimEnd("\0".ToArray());

            var serialNumber = Convert.ToUInt32(snString, Base);
            if (Base == 16 && serialNumber != 0xFFFFFFFF)
            {
                serialNumber *= 10;
            }
            return serialNumber;
        }
        #endregion

        #region EventHandler --------------------------------------------------------

        protected void OnConnectionChanged(ChangeType type, ITeensy changedDevice)
        {
            if (ConnectionChanged != null) ConnectionChanged(this, new ConnectionChangedEventArgs(type, changedDevice));
        }

        #endregion
    }

    public class ConnectionChangedEventArgs : EventArgs
    {
        public readonly TeensyWatcher.ChangeType changeType;
        public readonly ITeensy changedDevice;

        public ConnectionChangedEventArgs(TeensyWatcher.ChangeType type, ITeensy changedDevice)
        {
            this.changeType = type;
            this.changedDevice = changedDevice;
        }
    }
}






