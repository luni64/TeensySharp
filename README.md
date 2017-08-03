# TeensySharp
This library provides some helper classes for C# Windows applications which communicate to [PJRC Teensy](http://www.pjrc.com/teensy/index.html) boards. Currently the following common tasks are handled: 
- Finding all Teensies on the USB tree (get a list with entries for each found board containing its serialnumber and the name of the COM port to which it is connected)
- Provide information when a Teensy is connected or removed from the USB Tree
- Uploading of firmware (ihex files) from within the user application.

# Build
TeensySharp was developed using Microsoft VisualStudio 2013 Community Edition. The correspondig .sln file is contained in the repo. The library depends on the HIDLibrary from Mike O'Brian which can be found here: [https://github.com/mikeobrien/HidLibrary](https://github.com/mikeobrien/HidLibrary) and on MoreLinq ([https://code.google.com/p/morelinq/](https://code.google.com/p/morelinq/)). Both are available as NuGet packages. A download of the binaries from NuGet should start automatically when you build the solution. 

# Usage
## Finding Connected Teensies
### class TeensyWatcher
To obtain a list of all currently connected Teensies you can do the following:
```c#
var Watcher = new TeensyWatcher(); 
foreach (USB_Device Teensy in Watcher.ConnectedDevices)
{
    Console.WriteLine("Serialnumber {0}", Teensy.Serialnumber);
}
```
If you are interested in all boards running in USB-Serial mode you can write
```c#
var Watcher = new TeensyWatcher(); 
foreach (USB_Device Teensy in Watcher.ConnectedDevices.Where(t=>t.Type == USB_Device.type.UsbSerial))
{
    Console.WriteLine("Serialnumber {0}, on {1}", Teensy.Serialnumber, Teensy.Port);
}
```
The string *Teensy.Port* can then be used to construct a *SerialPort* object without having your users to guess which port number Windows assigned to your device today. 

In case you are looking for a board with a running bootloader and serialnumber 8324210
```c#
var Watcher = new TeensyWatcher(); 
var Teensy = Watcher.ConnectedDevices.FirstOrDefault(t=>(t.Serialnumber==8324210 && t.Type==USB_Device.type.HalfKay ));
```
The list of connected boards will be updated in the background whenever a Teensy is connected or removed. If you need a notification when the list changes you also can attach an eventhandler to the watcher:
```c#
...
var Watcher = new TeensyWatcher(); 
Watcher.ConnectionChanged += ConnectedTeensiesChanged;
...
void ConnectedTeensiesChanged(object sender, ConnectionChangedEventArgs e)
{
    string Port = e.changedDevice.Port;
    string SN = e.changedDevice.Serialnumber;
}
```
The repo contains a simple console application (*TeensyWacherConsole*) which demonstrates the use of the *TeensyWatcher* class and the eventhandler in more detail. 

## Firmware Uploading
### class SharpHexParser
The static *SharpHexParser* class is used to parse an Intel HEX stream and copy the result,i.e. the firmware into a flash image (which is a simple byte array). To generate an empty flash image with the correct size you can call a member of the SharpUploader as shown in the following code snippet. (*PJRC_Board* is an enum containing definitions for all supported PJRC Teensy Boards)

```c#
var Board = PJRC_Board.Teensy_31; 
var FlashImage = SharpUploader.GetEmptyFlashImage(Board);
SharpHexParser.ParseStream(File.OpenText("firmware.hex"), FlashImage);
```
The flash image now contains the bytes to be uploaded to the board. The actual upload will be be done by the class *SharpeUploader*.

### class SharpUploader
To upload the flash image to the board you use the static *SharpUploader* class. You can start the bootloader  by calling *StartHalfKay(uint Serialnumber)* (alternatively you can press the button on the board). After that you pass the flash image to the *Upload* member of the *SharpUploader* class. In the example below we upload the image to the first board we find. 
```c#
...
USB_Device Teensy = Watcher.ConnectedDevices.FirstOrDefault();
SharpUploader.StartHalfKay(Teensy.Serialnumber);
int result = SharpUploader.Upload(FlashImage, Board, Teensy.Serialnumber, reboot: true);
```
The reboot parameter determines if the board shall be rebooted after the upload - which is what you usually want. The *Upload* function returns the following codes: 
- **0** OK, upload finished without error
- **1** : No board with requested serialnumber and active HalfKay bootloader found 
- **2** : Error during upload of the firmware- 

The repo contains a  console application (*FirmwareDownloadConsole*) which demomstrates the use of the *TeensyWatcher* class.

## Status
- Compatibilty to T3.5 and T3.6 added
** ToBeDone:** WPF example showing the usage in a more realistic scenario

