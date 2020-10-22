using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Timers;
using System.Runtime.InteropServices;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    class debug
    {
        private static List<NetworkInterface> GetActiveInterface()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            var validInterfaces = new List<NetworkInterface>();
            foreach (var netInt in interfaces)
            {
                bool isLoopBack = NetworkInterfaceType.Loopback == netInt.NetworkInterfaceType;
                bool isTunnel = NetworkInterfaceType.Tunnel == netInt.NetworkInterfaceType;
                bool isUp = OperationalStatus.Up == netInt.OperationalStatus;
                bool isVirtualEth = netInt.Name.StartsWith("vEthernet");

                if (!isLoopBack && !isTunnel && isUp && !isVirtualEth) {
                    validInterfaces.Add(netInt);
                }
            }
            return validInterfaces;
        }

        static void Main(string[] args)
        {
            var monitors = new List<NetworkMonitor>();
            var interfaces = GetActiveInterface();
            foreach (var netInterface in interfaces) {
                var ipConfig = netInterface.GetIPProperties();
                foreach (var uni in ipConfig.UnicastAddresses) {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork) {
                        monitors.Add(new NetworkMonitor(uni.Address));
                    }
                }
            }
            
            while (true)
            {
            }
        }
    }

    public class NetworkMonitor
    {
        class MonitorObject
        {
            public Socket Socket;
            public byte[] Data;
            public const int BUFFER_SIZE = 0x4000;

            public MonitorObject(Socket s)
            {
                Socket = s;
                Data = new byte[BUFFER_SIZE];
            }
        }
       

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                MonitorObject monObj = ar.AsyncState as MonitorObject;
                Socket socket = monObj.Socket;
                if (null == socket) {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                    return;
                }

                SocketError err;
                int nReceived = socket.EndReceive(ar, out err);                
                byte[] bytesReceived = new byte[nReceived];
                if (nReceived < 1)
                {
                    monObj = new MonitorObject(socket);

                    socket.BeginReceive(monObj.Data,
                                        0,
                                        MonitorObject.BUFFER_SIZE,
                                        SocketFlags.None,
                                        new AsyncCallback(OnReceive),
                                        monObj);

                    return;
                }

                Buffer.BlockCopy(monObj.Data, 0, bytesReceived, 0, nReceived);
                var header = IPHeader.Parse(bytesReceived);

                if (header.HeaderVersion == HeaderVersion.IPv4) {
                    var ipv4 = header as IPv4Header;
                    Console.WriteLine(
                        ipv4.HeaderVersion + " - " +
                        ipv4.Protocol + " - " +
                        ipv4.SourceAddress.ToString() + " - " +
                        ipv4.DestinationAddress.ToString() + " - " +
                        (ipv4.FlagDoNotFragment ? "Do not fragment" :
                            (ipv4.FlagNoMoreFragments ? "No more fragments" : string.Empty)));
                }

                monObj = new MonitorObject(socket);
                socket.BeginReceive(monObj.Data, 
                                    0, 
                                    MonitorObject.BUFFER_SIZE, 
                                    SocketFlags.None, 
                                    new AsyncCallback(OnReceive), 
                                    monObj);
            }
            catch (Exception ex) {
                Console.WriteLine();
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.Message);
            }
        }


        public NetworkMonitor(IPAddress target)
        {
            var socket = new Socket(AddressFamily.InterNetwork, 
                                    SocketType.Raw, 
                                    ProtocolType.IP);

            socket.Bind(new IPEndPoint(target, 0));

            socket.SetSocketOption( SocketOptionLevel.IP, 
                                    SocketOptionName.HeaderIncluded, 
                                    true);

            byte[] bTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] bOut = new byte[4] { 1, 0, 0, 0 };

            MonitorObject monObj = new MonitorObject(socket);

            socket.ReceiveBufferSize = MonitorObject.BUFFER_SIZE;
            socket.IOControl(IOControlCode.ReceiveAll, bTrue, bOut);
            socket.BeginReceive(monObj.Data, 
                                0, 
                                MonitorObject.BUFFER_SIZE, 
                                SocketFlags.None, 
                                new AsyncCallback(OnReceive), 
                                monObj);
        }
    }
}
