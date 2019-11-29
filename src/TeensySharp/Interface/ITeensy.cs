using HidLibrary;
using System;


namespace TeensySharp.Interface
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

    public enum USBtype
    {
        UsbSerial,
        HalfKay,
        HID,
    }

    public interface ITeensy
    {
        USBtype usbType { get; }
        uint serialnumber { get; }
        String port { get; }
        PJRC_Board boardType { get; }
        String boardId { get; }



        bool StartBootloader();
        bool Upload(IFirmware firmware, bool checkType = true, bool reset = true);
        bool Reset();

      //  HidDevice hidDevice { get; }
    }
}
