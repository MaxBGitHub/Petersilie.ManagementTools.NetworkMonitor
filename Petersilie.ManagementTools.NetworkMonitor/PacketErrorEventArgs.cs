using System;
using System.Net;
using System.Net.Sockets;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Contains detailed information of a socket error.
    /// </summary>
    public class PacketErrorEventArgs : EventArgs
    {
        /// <summary>
        /// Exception that was caught while trying 
        /// to process a Socket connection.
        /// </summary>
        public Exception Exception { get; }
        /// <summary>
        /// The socket that raised the error.
        /// </summary>
        public Socket Socket { get; }
        /// <summary>
        /// IP address bound to Socket.
        /// </summary>
        public IPAddress IPAddress { get; }
        /// <summary>
        /// Port which is bound to Socket.
        /// </summary>
        public int Port { get; }


        /// <summary>
        /// Initializes a new instance of the PacketErrorEventArgs.
        /// </summary>
        /// <param name="exception">The Exception that was caught</param>
        /// <param name="socket">Socket that raised the error</param>
        /// <param name="ip">IP address bound to Socket</param>
        /// <param name="port">Port which is bound to Socket</param>
        public PacketErrorEventArgs(Exception  exception, 
                                    Socket     socket, 
                                    IPAddress  ip, 
                                    int        port)
        {
            Exception   = exception;
            Socket      = socket;
            IPAddress   = ip;
            Port        = port;
        }
    }
}
