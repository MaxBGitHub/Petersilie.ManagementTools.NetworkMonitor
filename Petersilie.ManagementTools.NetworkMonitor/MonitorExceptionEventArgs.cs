using System;
using System.Net;
using System.Net.Sockets;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public class MonitorExceptionEventArgs : EventArgs
    {
        // TODO :: Implement Monitor exception class.
        public Exception    Error       { get; }
        public Socket       Socket      { get; }
        public IPAddress    IPAddress   { get; }
        public int          Port        { get; }


        public MonitorExceptionEventArgs(Exception  ex, 
                                         Socket     s, 
                                         IPAddress  ip, 
                                         int        port)
        {
            Error       = ex;
            Socket      = s;
            IPAddress   = ip;
            Port        = port;
        }
    }
}
