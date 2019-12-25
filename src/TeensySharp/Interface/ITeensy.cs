using HidLibrary;
using System;
using System.ComponentModel;

namespace lunOptics.TeensySharp
{
    public enum PJRC_Board
    {
        Teensy40,
        Teensy36,
        Teensy35,
        Teensy_31_2,
        Teensy_30,
        Teensy_LC,
        Teensy_2pp,
        Teensy_2,       
        unknown,
    }

    public enum UsbSubType
    {
        none,
        RawHID,       
        SerialKeyboardMouseJoystick,
        SerialMIDI,
        SerialMIDIAudio,
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

    public enum UsbType
    {
        disconnected,
        HalfKay,
        HID,        
        Serial,
        unknown,
    }

    public interface ITeensy: INotifyPropertyChanged
    {
        UsbType UsbType { get; }
        UsbSubType UsbSubType { get; }


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
