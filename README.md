# Petersilie.ManagementTools.NetworkMonitor
Monitors network traffic on one or more network interfaces.


## NetworkMonitor
The NetworkMonitor class is the main component of this library.

It monitors the network traffic on the IP level and receives raw streams of data.

Packets can be received and processed by subscribing to the NetworkMonitor.PacketReceived event.
Errors which occur during AsyncCallbacks of the Socket can be caught and processed by subscribing to the NetworkMonitor.OnError event.
The NetworkMonitor will not stop when it catches any exception in the Sockets AsyncCallback.

This repository contains the project/code for building a .NET Framework DLL.
It is not a command-line utility or contains any GUI.


## Installation
Download the project and build it.
Add the projects Dll as a reference to a other project in which you want to use the NetworkMonitor class.

## Usage
You can bind a specific IP and port to the NetworkMonitor by using one of its, the default port is 0.
3 constructors
``` csharp
NetworkMonitor(IPAddress)
NetworkMonitor(IPAddress, int)
NetworkMonitor(IPEndPoint)
```
Example initialization:
```csharp
System.Net.IPAddress addr = IPAddress.Parse("127.0.0.1");
var netMonitor = new NetworkMonitor(addr);
netMonitor.PacketReceived += NetworkMonitor_PacketReceived;
netMonitor.OnError += NetworkMonitor_OnError;
netMonitor.Begin();
```

You can also bind all active interfaces by using the static BindInterfaces() method.
It returns an array of NetworkMonitor objects.
``` csharp
NetworkMonitor[] netMonitors = NetworkMonitor.BindInterfaces();
```

