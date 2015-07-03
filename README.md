# TeensySharp
This library provides some helper classes for C# Windows applications which communicate to [PJRC Teensy](http://www.pjrc.com/teensy/index.html) boards. Currently the following common tasks are handled: 
- Finding all Teensies on the USB Tree (board list with entries for each found board containing its serialnumber and the name of the COM port to which it is connected)
- Provide information when a Teensy is connected or removed from the USB Tree
- Uploading of firmware (ihex files) to a connected Teensy board

#Build
TeensySharp was developed with Microsoft VisualStudio 2013 Community Edition. The correspondig .sln file is contained in the repo. The library depends on the HIDLibrary from Mike O'Brian which can be found here: [https://github.com/mikeobrien/HidLibrary](https://github.com/mikeobrien/HidLibrary) and on MoreLinq ([https://code.google.com/p/morelinq/](https://code.google.com/p/morelinq/)). Both are available via NuGet. A download of the binaries from NuGet should start automatically when you build the solution. 

The solution also includes the following two demo console applications showing the application of the classes.

##Documentation
###class TeensyWatcher
To obtain a list of currently connected Teensies you can do the following
```c#
var Watcher = new TeensyWatcher(); 
foreach (var Teensy in Watcher.ConnectedDevices)
{
    Console.WriteLine("Serialnumber {0}, on port {1}", Teensy.Serialnumber, Teensy.Port);
}
```
If you need a notification whenever a Teensy is connected or removed you can attach an eventhandler
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
The solution contains a console application which demomstrates the use of the "TeensyWatcher" class.

###class SharpHexParser
