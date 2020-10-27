using System;
using System.Net;
using System.Net.Sockets;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public class PacketEventArgs : EventArgs
    {
        public byte[]       Packet          { get; } // Raw packet bytes.
        public IPAddress    SocketAddress   { get; } // Bound Socket IP.
        public int          SocketPort      { get; } // Bound Socket Port.
        public SocketError  SocketError     { get; } // Caught Error.
        public IPVersion    Version         { get; } // Version of header.


        public PacketEventArgs( byte[]      packet, 
                                IPAddress   sAddrs, 
                                int         sPort, 
                                SocketError sErr)
        {
            Packet          = packet;
            SocketAddress   = sAddrs;
            SocketPort      = sPort;
            SocketError     = sErr;
            Version         = IPHeader.GetVersion(packet);
        }


        public PacketEventArgs( byte[]      packet, 
                                IPAddress   sAddrs, 
                                int         sPort)
        {
            Packet          = packet;
            SocketAddress   = sAddrs;
            SocketPort      = sPort;
            SocketError     = SocketError.Success;
            Version         = IPHeader.GetVersion(packet);
        }
    }
}
