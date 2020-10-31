using HidLibrary;
using System;
using System.ComponentModel;

namespace lunOptics.libTeensySharp
{
    public enum ErrorCode
    {
        OK,
        Upload_FirmwareMismatch,
        RebootError,
        ResetError,
        HidComError,
        Upload_Timeout,
        Unexpected,

    }

    public enum PJRC_Board
    {
        T4_1,
        T4_0,
        T3_6,
        T3_5,
        T3_2,
        T3_0,
        T_LC,
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
        COMPOSITE,
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
