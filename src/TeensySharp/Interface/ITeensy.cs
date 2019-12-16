using HidLibrary;
using System;
using System.ComponentModel;

namespace lunOptics.TeensySharp
{
    public enum PJRC_Board
    {
        Teensy_40,
        Teensy_36,
        Teensy_35,
        Teensy_31_2,
        Teensy_30,
        Teensy_LC,
        Teensy_2pp,
        Teensy_2,       
        unknown,
    }

    public enum UsbSubTypes
    {
        none,
        RawHID,       
        Serial_Keyboard_Mouse_Joystick,
        Serial_MIDI,
        Serial_MIDI_Audio,
        Keyboard,
        Keyboard_Touchscreen,
        Keyboard_Mouse_Joystick,
        Keyboard_Mouse_Touchscreen,
        MIDI,
        FlightSim,
        MTP_Disk,
        Audio,
        Everything,
    }

    public enum UsbTypes
    {
        disconnected,
        HalfKay,
        HID,        
        Serial,
        unknown,
    }

    public interface ITeensy: INotifyPropertyChanged
    {
        UsbTypes UsbType { get; }
        UsbSubTypes UsbSubType { get; }


        uint Serialnumber { get; }
        String Port { get; }
        PJRC_Board BoardType { get; }
        String BoardId { get; }
               
        bool Reboot();
        bool Upload(IFirmware firmware, bool checkType = true, bool reset = true);
        bool Reset();

      //  HidDevice hidDevice { get; }
    }
}
