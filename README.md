# TeensySharp
This library provides some helper classes for C# Windows applications which communicate to [PJRC Teensy](http://www.pjrc.com/teensy/index.html) boards. Currently the following common tasks are handled: 
- Finding all Teensies on the USB Tree (board list with entries for each found board containing its serialnumber and the name of the COM port to which it is connected)
- Provide information when a Teensy is connected or removed from the USB Tree
- Uploading of firmware (ihex files) to a connected Teensy board

#Build
TeensySharp was developed using Microsoft VisualStudio 2013 Community Edition. The correspondig .sln file is contained in the repo. The library depends on the HIDLibrary from Mike O'Brian which can be found here: [https://github.com/mikeobrien/HidLibrary](https://github.com/mikeobrien/HidLibrary) and on MoreLinq ([https://code.google.com/p/morelinq/](https://code.google.com/p/morelinq/)). Both are available as NuGet packages. A download of the binaries from NuGet should start automatically when you build the solution. 

#Usage
## Finding Connected Teensies
###class TeensyWatcher
To obtain a list of currently connected Teensies you can do the following
```c#
var Watcher = new TeensyWatcher(); 
foreach (var Teensy in Watcher.ConnectedDevices)
{
    Console.WriteLine("Serialnumber {0}, on port {1}", Teensy.Serialnumber, Teensy.Port);
}
```
The reported *Port* member of the *TeensyWatcher* class can then be used to construct a *SerialPort* object without having your users to guess which port number Windows assigned to your device today. The list of connected boards will be updated in the background whenever a Teensy is connected or removed. If you need a notification when the list changes you can attach an eventhandler to the watcher:
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
The repo contains a simple console application (*TeensyWacherConsole*) which demomstrates the use of the *TeensyWatcher* class.
##Firmware Uploading
###class SharpHexParser
The static *SharpHexParser* class is used to parse a stream containing an Intel HEX file and copy the result,i.e. the firmware into a flash image file (simple byte array). To generate an empty flash image with the correct size you can call a member of the SharpUploader as shown in the following code snippet. (PJRC_Board is an enum containing definitions for all PJRC Teensy Boards)

```c#
var Board = PJRC_Board.Teensy_31; 
var FlashImage = SharpUploader.GetEmptyFlashImage(Board);
SharpHexParser.ParseStream(File.OpenText("firmware.hex"), FlashImage);
```
The flash image now contains the bytes to be downloaded to the board. The actual download will be be done by the class SharpeUploader.

###class SharpUploader
To upload a flash image to the board you only need to pass the flash image to the static *SharpUploader* class: 
```c#
 int result = SharpUploader.Upload(FlashImage, Board, reboot: true);
```
The reboot parameter determines if the board shall be rebooted after the download which is what you usually want. The *Upload* member returns the following codes: 
- **0** OK, download finished without error
- **1** : No board with active HalfKay bootloader found (press the programming button on the board to activate HalfKay)
- **2** : Error during download of the firmware- 

The repo contains a  console application (*FirmwareDownloadConsole*) which demomstrates the use of the *TeensyWatcher* class.

##Status
- Since I only own **Teensy 3.1** boards I could not test the library for the other PJRC boards. Any input / error reports / improvements welcome. 
- **ToBeDone:** WPF example showing the usage in a more realistic scenario
- **ToBeDone:** Find out how to activate HalfKay without pressing the program button on the boards
