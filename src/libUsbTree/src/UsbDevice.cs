using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static lunOptics.libUsbTree.NativeWrapper;

namespace lunOptics.libUsbTree
{
    public class UsbDevice : IUsbDevice, INotifyPropertyChanged 
    {
        #region public properties and methods ---------------------------------

        public int node { get; private set; }
        public string DeviceInstanceID { get; private set; }
        public List<string> HardwareIDs { get; private set; }
        public string Description { get; protected set; }
        public bool IsConnected { get;  set; }
        public Guid ClassGuid { get; private set; }
        public string ClassDescription { get; private set; }
        protected string SnString { get; private set; }
        public int Vid { get; private set; }
        public int Pid { get; private set; }
        public int Rev { get; private set; }
        public int Mi { get; private set; }
        public uint HidUsageID { get; private set; }
        public bool IsInterface { get; private set; }
        public bool IsUsbFunction { get; private set; }
        public ObservableCollection<IUsbDevice> children { get; } = new ObservableCollection<IUsbDevice>();
        public ObservableCollection<IUsbDevice> functions { get; } = new ObservableCollection<IUsbDevice>();
        public ObservableCollection<IUsbDevice> interfaces { get; } = new ObservableCollection<IUsbDevice>();

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual bool isEqual(InfoNode other)
        {
            return other != null && this.DeviceInstanceID == other.devInstId;
        }
        public virtual void update(InfoNode info)
        {            
            doUpdate(info); // calling virtual functions from constructors is a bad idea => call by indirection. 
        }

        public override string ToString()
        {
            return $"{ClassDescription} {Description} ({Vid:X4}/{Pid:X4}) #{SnString}";
        }
        #endregion

        #region construction -------------------------------------------
        private void doUpdate(InfoNode info) 
        {
            if (info == null) return;

           // Trace.WriteLine("update usbdevice");

            foreach (var childInfo in info.children)
            {
                if (childInfo.isInterface)
                    interfaces.AddIfNew(UsbTree.deviceFactory.MakeOrUpdate(childInfo));
                else if (childInfo.isUsbFunction)
                    functions.AddIfNew(UsbTree.deviceFactory.MakeOrUpdate(childInfo));
                else
                    children.AddIfNew(UsbTree.deviceFactory.MakeOrUpdate(childInfo));
            }

            foreach (var child in children.ToList())
            {
                if (!info.children.Any(i => ((UsbDevice) child).isEqual(i)))  // if child is currently disconnected
                {
                    child.interfaces.Clear();
                    child.functions.Clear();
                    child.children.Clear();
                    children.Remove(child);
                }
            }

            if (info.node >= 0) // root node only updates its children
            {
                if (!String.IsNullOrEmpty(info.devInstId)) // driver not yet loaded or other error (happened)
                {
                    DeviceInstanceID = info.devInstId;
                    Vid = info.vid;
                    Pid = info.pid;
                    Mi = info.mi;
                    IsInterface = info.isInterface;
                    IsUsbFunction = info.isUsbFunction;
                    SnString = info.serNumStr;
                    node = info.node;

                    Description = cmGetNodePropStrg(info.node, DevPropKeys.Name) ?? "ERR: No Value";
                    ClassGuid = cmGetNodePropGuid(info.node, DevPropKeys.DeviceClassGuid);
                    ClassDescription = cmGetNodePropStrg(info.node, DevPropKeys.DeviceClass) ?? "ERR: No Value";
                    HardwareIDs = cmGetNodePropStringList(info.node, DevPropKeys.HardwareIds);

                    string s = @"HID_DEVICE_UP:([0-9A-F]{4})_U:([0-9A-F]{4})";
                    var match = HardwareIDs.Select(id => Regex.Match(id, s, RegexOptions.IgnoreCase)).FirstOrDefault(m => m.Success);
                    if (match != null)
                    {
                        HidUsageID = (uint)Convert.ToUInt16(match.Groups[1].Value, 16) << 16 | Convert.ToUInt16(match.Groups[2].Value, 16);
                    }

                    s = @"REV[_]?([0-9A-F]{4})";
                    match = HardwareIDs.Select(id => Regex.Match(id, s, RegexOptions.IgnoreCase)).FirstOrDefault(m => m.Success);
                    if (match != null)
                    {
                        Rev = Convert.ToInt32(match.Groups[1].Value, 16);
                    }
                }
                else
                {
                    DeviceInstanceID = "ERR: No DeviceInstanceID";
                    Description = "ERR: No Description";
                }
                OnPropertyChanged("");
            }

        }

        public UsbDevice(InfoNode info = null)
        {
            doUpdate(info);
        }

        #endregion
               
        #region INotifyPropertyChanged
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                OnPropertyChanged(name);
            }
        }
        protected void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion
    }

    public static partial class MyExtensions
    {
        public static bool AddIfNew<T>(this Collection<T> collection, T val)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (!collection.Contains(val))
            {
                collection.Add(val);
                return true;
            }
            return false;
        }
    }
}
