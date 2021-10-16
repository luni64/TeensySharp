using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace lunOptics.libUsbTree
{
    public interface IUsbDevice : INotifyPropertyChanged 
    {
        string DeviceInstanceID { get; }
        List<string> HardwareIDs { get; }
        string Description { get; }
        bool IsConnected { get; }
        int Vid { get; }
        int Pid { get; }
        int Rev { get; }
        int Mi { get; }
        bool IsInterface { get; }
        bool IsUsbFunction { get; }
        uint HidUsageID { get; }

        ObservableCollection<IUsbDevice> children { get; }
        ObservableCollection<IUsbDevice> functions { get; }
        ObservableCollection<IUsbDevice> interfaces { get; }
    }
}
