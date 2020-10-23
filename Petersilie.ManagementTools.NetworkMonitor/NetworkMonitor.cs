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
using Petersilie.ManagementTools.NetworkMonitor.Header;


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


        private static void HeaderReceived(object sender, IPHeaderEventArgs e)
        {
            if (e.Header == null) return;
            var h = e.Header as IPv4Header;
            if (h.Protocol == Protocol.UDP)
            {                
                if (h.Data.Length > 0) {
                    var udpHeader = new UDPHeader(h.Data);
                    if (udpHeader.Data.Length > 0)
                    {
                        //Console.WriteLine(
                        //    h.SourceAddress + " - " +
                        //    udpHeader.SourcePort + " - " +
                        //    h.DestinationAddress + " - " +
                        //    udpHeader.DestinationPort + " - " +
                        //    $"UDP payload length: {udpHeader.Data.Length}");
                    }                    
                }
            }
            else if (h.Protocol == Protocol.TCP)
            {
                if (h.Data.Length > 0)
                {
                    var tcpHeader = new TCPHeader(h.Data);
                    if (tcpHeader.Data.Length > 0)
                    {
                        //Console.WriteLine(
                        //    h.SourceAddress + " - " +
                        //    tcpHeader.SourcePort + " - " +
                        //    h.DestinationAddress + " - " +
                        //    tcpHeader.DestinationPort + " - " +
                        //    $"TCP payload length: {tcpHeader.Data.Length}");
                    }
                }
            }
            else if (h.Protocol == Protocol.ICMP)
            {
                if (h.Data.Length > 0)
                {
                    var icmpHeader = new ICMPHeader(h.Data);
                    Console.WriteLine(
                        icmpHeader.Type + " - " +
                        icmpHeader.Typename + " - " +
                        icmpHeader.Code + " - " +
                        icmpHeader.Checksum);
                }
            }
            //Console.WriteLine(
            //        h.IPVersion + " - " +
            //        h.Protocol + " - " +
            //        h.SourceAddress + " - " +
            //        h.DestinationAddress + " - " +
            //        h.Data.Length);
        }


        static void Main(string[] args)
        {
            var monitors = new List<NetworkMonitor>();
            var interfaces = GetActiveInterface();
            foreach (var netInterface in interfaces) {
                var ipConfig = netInterface.GetIPProperties();
                foreach (var uni in ipConfig.UnicastAddresses) {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork) {
                        var mon = new NetworkMonitor(uni.Address);
                        mon.IPv4HeaderReceived += HeaderReceived;
                        mon.Begin();
                        monitors.Add(mon);
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
        public IPAddress IPAddress { get; }

        public int Port { get; }


        private event EventHandler<IPHeaderEventArgs> onIPv4HeaderReceived;
        public event EventHandler<IPHeaderEventArgs> IPv4HeaderReceived
        {
            add {
                onIPv4HeaderReceived += value;
            }
            remove {
                onIPv4HeaderReceived -= value;
            }
        }

        protected virtual void OnIPv4HeaderReceived(IPHeaderEventArgs ipArgs)
        {
            onIPv4HeaderReceived?.Invoke(this, ipArgs);
        }


        private bool _continue = true;


        /* ==========================
        ** =        RCVALL_ON       =
        ** ==========================
        **
        ** Used for Socket.IOControl params optionInValue and optionOutValue.
        ** These values are documented in SIO_RCVALL Control Code for WSAIoctl.
        ** In WSAIoctl these are refered to as lpvInBuffer and lpvOutBuffer.
        ** They refere to the enum RCVALL_VALUE/*PRCVALL_VALUE in mstcpip.h.
        **
        ** Here is the exact definition:
        **
        *   //
        *   // Values for use with SIO_RCVALL* options
        *   //
        **  typedef enum {
        **      RCVALL_OFF              = 0,
        **      RCVALL_ON               = 1,
        **      RCVALL_SOCKETLEVELONLY  = 2,
        **      RCVALL_IPLEVEL          = 3,
        **  } RCVALL_VALUE, *PRCVALL_VALUE;
        ** 
        ** This little byte array is extremly important for us.
        ** SIO_RCVALL is what enables the NIC to be sniffed,
        ** and what level of sniffing is allowed (to some extend).
        */
        static byte[] RCVALL_ON = new byte[4] {
            1,
            0,
            0,
            0
        };


        private void TryRelease(Socket s)
        {
            if (null == s) {
                return;
            }

            try {
                s.Close(500);
                s.Dispose();
                s = null;
            } catch { }
        }


        private void OnReceive(IAsyncResult ar)
        {
            SocketStateObject monObj = ar.AsyncState as SocketStateObject;
            if (null == monObj) {
                _continue = false;
                return;
            }

            if ( !_continue ) {
                TryRelease(monObj.Socket);
                return;
            }

            try
            {
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
                    if (SocketError.Success == err) {
                        err = SocketError.NoData;
                    }

                    var ipArgs = new IPHeaderEventArgs( null, 
                                                        IPAddress, 
                                                        Port, 
                                                        err);

                    OnIPv4HeaderReceived(ipArgs);

                    monObj = new SocketStateObject(socket);
                    socket.BeginReceive(monObj.Data, 0,
                                        SocketStateObject.BUFFER_SIZE,
                                        SocketFlags.None,
                                        new AsyncCallback(OnReceive),
                                        monObj);

                    return;
                }

                Buffer.BlockCopy(monObj.Data, 0, bytesReceived, 0, nReceived);
                var header = IPHeader.Parse(bytesReceived);

                if (header.IPVersion == IPVersion.IPv4)
                {
                    var ipArgs = new IPHeaderEventArgs( header, 
                                                        IPAddress, 
                                                        Port);
                    OnIPv4HeaderReceived(ipArgs);
                }

                monObj = new SocketStateObject(socket);
                socket.BeginReceive(monObj.Data, 
                                    0,
                                    SocketStateObject.BUFFER_SIZE, 
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


        public void Begin()
        {
            var socket = new Socket(AddressFamily.InterNetwork, 
                                    SocketType.Raw, 
                                    ProtocolType.IP);

            socket.Bind(new IPEndPoint(IPAddress, Port));

            socket.SetSocketOption( SocketOptionLevel.IP, 
                                    SocketOptionName.HeaderIncluded, 
                                    true);

            SocketStateObject monObj = new SocketStateObject(socket);
            socket.ReceiveBufferSize = SocketStateObject.BUFFER_SIZE;
            socket.IOControl(IOControlCode.ReceiveAll, RCVALL_ON, RCVALL_ON);
            socket.BeginReceive(monObj.Data, 0,
                                SocketStateObject.BUFFER_SIZE, 
                                SocketFlags.None, 
                                new AsyncCallback(OnReceive), 
                                monObj);
        }


        public void Stop()
        {
            _continue = false;
        }


        public NetworkMonitor(IPAddress target)
        {
            IPAddress = target;
            Port = 0;
        }
    }
}
