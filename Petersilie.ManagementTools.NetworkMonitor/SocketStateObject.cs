﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public class SocketStateObject
    {
        public const int BUFFER_SIZE = 0x4000;

        public Socket Socket;
        public byte[] Data;        

        public SocketStateObject(Socket s)
        {
            Socket = s;
            Data = new byte[BUFFER_SIZE];
        }
    }
}