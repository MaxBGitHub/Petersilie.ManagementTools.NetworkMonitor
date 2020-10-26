﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

using Petersilie.ManagementTools.NetworkMonitor.Header;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public class PacketEventArgs : EventArgs
    {
        public IPHeader     Header          { get; } // Parsed header.
        public IPAddress    SocketAddress   { get; } // Bound Socket IP.
        public int          SocketPort      { get; } // Bound Socket Port.
        public SocketError  SocketError     { get; } // Caught Error.
        public IPVersion    Version         { get; } // Version of header.


        public PacketEventArgs( IPHeader    header, 
                                IPAddress   sAddrs, 
                                int         sPort, 
                                SocketError sErr)
        {
            if (null != header) {
                Header  = header;
                Version = header.IPVersion;
            } /* Check header for null. */

            SocketAddress   = sAddrs;
            SocketPort      = sPort;
            SocketError     = sErr;            
        }


        public PacketEventArgs( IPHeader    header, 
                                IPAddress   sAddrs, 
                                int         sPort)
        {
            if (null != header) {
                Header  = header;
                Version = header.IPVersion;
            } /* Check header for null. */

            SocketAddress   = sAddrs;
            SocketPort      = sPort;
            SocketError     = SocketError.Success;            
        }
    }
}