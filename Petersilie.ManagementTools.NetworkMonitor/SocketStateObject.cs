using System.Net.Sockets;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    internal class SocketStateObject
    {
        public Socket   Socket;     // Socket of connection.
        public byte[]   Data;       // Empty or filled data buffer.
        public int      BufferSize; // Initial size of data buffer.

        public SocketStateObject(Socket socket, int bufferSize)
        {
            Socket      = socket;
            Data        = new byte[bufferSize];
            BufferSize  = bufferSize;
        }
    }
}
