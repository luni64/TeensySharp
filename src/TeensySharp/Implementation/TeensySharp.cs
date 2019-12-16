using HidLibrary;
using lunOptics.TeensySharp.Implementation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;

namespace lunOptics.TeensySharp
{
    public enum ChangeType
    {
        add,
        remove
    }

    public static class TeensySharp
    {
        #region Construction / Destruction ------------------------------------------
        static TeensySharp()
        {
            ConnectedBoards = new List<ITeensy>();
            CachedBoards = new List<ITeensy>();

            using (var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity WHERE DeviceID LIKE " + vidStr))
            {
                foreach (var mgmtObject in searcher.Get())
                {
                    var device = MakeDevice(mgmtObject);
                    if (device != null)
                    {
                        ConnectedBoards.Add(device);
                        CachedBoards.Add(device);
                    }
                }
            }
            StartWatching();
        }
        #endregion

        #region Properties and Events -----------------------------------------------
        public static List<ITeensy> ConnectedBoards { get; }

        public static SynchronizationContext ctx { get; set; } = null;

        static public void SynchronizationContext(SynchronizationContext _ctx)
        {
            ctx = _ctx;
        }

        static public event EventHandler<ConnectedBoardsChangedArgs> ConnectedBoardsChanged;
        #endregion

        #region Port Watching  ------------------------------------------------------

        private static ManagementEventWatcher CreateWatcher;
        private static ManagementEventWatcher DeleteWatcher;

        private static void StartWatching()
        {
            StopWatching(); // Just to make sure 

            DeleteWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceDeletionEvent",
                    Condition = "TargetInstance ISA 'Win32_PnPEntity'",
                    WithinInterval = TimeSpan.FromSeconds(1), //Todo: make the interval settable
                },
            };
            DeleteWatcher.EventArrived += watcherEvent;
            DeleteWatcher.Start();

            CreateWatcher = new ManagementEventWatcher
            {
                Query = new WqlEventQuery
                {
                    EventClassName = "__InstanceCreationEvent",
                    Condition = "TargetInstance ISA 'Win32_PnPEntity'",
                    WithinInterval = TimeSpan.FromSeconds(1), //Todo: make the interval settable
                },
            };
            CreateWatcher.EventArrived += watcherEvent;
            CreateWatcher.Start();
        }

        private static void StopWatching()
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


        private static void watcherEvent(object sender, EventArrivedEventArgs e)
        {
            var device = MakeDevice((ManagementBaseObject)e.NewEvent["TargetInstance"]);
            if (device != null)
            {
                var cached = (Teensy)CachedBoards.FirstOrDefault(t => t.Serialnumber == device.Serialnumber);
                if (cached == null) CachedBoards.Add(device);
                else
                {
                    cached.UsbType = device.UsbType;
                    device = cached; // use cached instance to keep references stored by user intact
                }

                ChangeType type = e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent" ? ChangeType.add : ChangeType.remove;

                if (type == ChangeType.add)
                {

                    ConnectedBoards.Add(device);
                }
                else
                {
                    device.UsbType = UsbTypes.disconnected;  // user may hold reference to the board outside the list
                    ConnectedBoards.Remove(device);
                }
                ConnectedBoardsChanged.ThreadAwareRaise(ctx, null, new ConnectedBoardsChangedArgs(type, device));
            }
        }

        private static List<ITeensy> CachedBoards;

        #endregion

        #region Helpers


        internal static Teensy MakeDevice(ManagementBaseObject mgmtObj)
        {

            var DeviceIdParts = ((string)mgmtObj["PNPDeviceID"]).Split("\\".ToArray());
            if (DeviceIdParts[0] != "USB") return null;

            var vidPidMi = DeviceIdParts[1].Split("&".ToArray());
            if (vidPidMi.Length != 2) return null;  // we are only interested in devices, not interfaces


            int vid = Convert.ToInt32(vidPidMi[0].Substring(4, 4), 16);
            int pid = Convert.ToInt32(vidPidMi[1].Substring(4, 4), 16);

            if (vid != pjrcVid) return null;

            Debug.WriteLine($"md ({pid:X})");

            PJRC_Board board;

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

                return new Teensy
                {
                    UsbType = UsbTypes.HalfKay,
                    UsbSubType = UsbSubTypes.none,
                    Port = "",
                    Serialnumber = serNum,
                    BoardType = board,
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

                UsbTypes t;
                UsbSubTypes st;


                switch (pid)
                {
                    case 0x0476:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.Everything;
                        break;
                    case 0x0482:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.Keyboard_Mouse_Joystick;
                        break;
                    case 0x0483:
                        t = UsbTypes.Serial;
                        st = UsbSubTypes.none;
                        break;
                    case 0x0485:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.MIDI;
                        break;
                    case 0x0486:
                        st = UsbSubTypes.RawHID;
                        t = UsbTypes.HID;
                        break;
                    case 0x0487:
                        t = UsbTypes.Serial;
                        st = UsbSubTypes.Serial_Keyboard_Mouse_Joystick;
                        break;
                    case 0x0488:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.FlightSim;
                        break;
                    case 0x0489:
                        t = UsbTypes.Serial;
                        st = UsbSubTypes.Serial_MIDI;
                        break;
                    case 0x048A:
                        t = UsbTypes.Serial;
                        st = UsbSubTypes.Serial_MIDI_Audio;
                        break;
                    case 0x04D0:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.Keyboard;
                        break;
                    case 0x04D1:
                        t = UsbTypes.unknown;
                        st = UsbSubTypes.MTP_Disk;
                        break;
                    case 0x04D2:
                        t = UsbTypes.unknown;
                        st = UsbSubTypes.Audio;
                        break;
                    case 0x04D3:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.Keyboard_Touchscreen;
                        break;
                    case 0x04D4:
                        t = UsbTypes.HID;
                        st = UsbSubTypes.Keyboard_Mouse_Touchscreen; break;

                    default:
                        t = UsbTypes.unknown;
                        st = UsbSubTypes.none; break;
                }


                var l = HidDevices.Enumerate(vid, pid);
                foreach (HidDevice d in l)
                {
                    var sn = GetSerialNumber(d, 10);

                    d.ReadManufacturer(out byte[] man);
                    string snString = System.Text.Encoding.Unicode.GetString(man).TrimEnd("\0".ToArray());
                    d.ReadProduct(out byte[] prd);
                    string ssnString = System.Text.Encoding.Unicode.GetString(prd).TrimEnd("\0".ToArray());
                    d.ReadFeatureData(out byte[] features);
                    string sfeatures = System.Text.Encoding.Unicode.GetString(features).TrimEnd("\0".ToArray());

                    
                    


                }

                return new Teensy
                {
                    Serialnumber = serNum,
                    BoardType = board,
                    UsbType = t,
                    UsbSubType = st,
                    Port = (pid == serPid ? (((string)mgmtObj["Caption"]).Split("()".ToArray()))[1] : ""),
                    hidDevice = (pid == serPid ? null : HidDevices.Enumerate(vid, pid).FirstOrDefault(d => GetSerialNumber(d, 10) == serNum)),
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

        private const int pjrcVid = 0x16C0;
        private const int serPid = 0x483;
        private const int halfKayPid = 0x478;
        private static readonly string vidStr = "'%USB_VID[_]" + pjrcVid.ToString("X") + "%'";

        #endregion       
    }

    public class ConnectedBoardsChangedArgs : EventArgs
    {
        public readonly ChangeType changeType;
        public readonly ITeensy changedDevice;

        public ConnectedBoardsChangedArgs(ChangeType type, ITeensy changedDevice)
        {
            this.changeType = type;
            this.changedDevice = changedDevice;
        }
    }
}






