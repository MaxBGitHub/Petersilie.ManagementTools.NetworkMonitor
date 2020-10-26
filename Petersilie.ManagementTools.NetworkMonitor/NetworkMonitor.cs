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
    public class NetworkMonitor : IDisposable
    {
        public IPAddress IPAddress { get; }

        public int Port { get; }


        private event EventHandler<MonitorExceptionEventArgs> onError;
        public event EventHandler<MonitorExceptionEventArgs> OnError
        {
            add {
                onError += value;
            }
            remove {
                onError -= value;
            }
        }

        protected virtual void OnErrorInternal(MonitorExceptionEventArgs e)
        {
            onError?.Invoke(this, e);
        }


        private event EventHandler<IPHeaderEventArgs> onIPHeaderReceived;
        public event EventHandler<IPHeaderEventArgs> IPHeaderReceived
        {
            add {
                onIPHeaderReceived += value;
            }
            remove {
                onIPHeaderReceived -= value;
            }
        }

        protected virtual void OnIPHeaderReceived(IPHeaderEventArgs ipArgs)
        {
            onIPHeaderReceived?.Invoke(this, ipArgs);
        }


        private bool _continue = true;
        private Socket _socket = null;


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

                    OnIPHeaderReceived(ipArgs);

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
                    OnIPHeaderReceived(ipArgs);
                }

                monObj = new SocketStateObject(socket);
                socket.BeginReceive(monObj.Data, 0,
                                    SocketStateObject.BUFFER_SIZE, 
                                    SocketFlags.None, 
                                    new AsyncCallback(OnReceive), 
                                    monObj);
            }
            catch (Exception ex)
            {
                var errArgs = new MonitorExceptionEventArgs(
                    ex, _socket, IPAddress, Port);

                OnErrorInternal(errArgs);
            }
        }


        public void Begin()
        {
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Raw,
                                     ProtocolType.IP);

                _socket.Bind(new IPEndPoint(IPAddress, Port));

                _socket.SetSocketOption(SocketOptionLevel.IP,
                                        SocketOptionName.HeaderIncluded,
                                        true);

                SocketStateObject monObj = new SocketStateObject(_socket);
                _socket.ReceiveBufferSize = SocketStateObject.BUFFER_SIZE;
                _socket.IOControl(IOControlCode.ReceiveAll, RCVALL_ON, RCVALL_ON);
                _socket.BeginReceive(monObj.Data, 0,
                                    SocketStateObject.BUFFER_SIZE,
                                    SocketFlags.None,
                                    new AsyncCallback(OnReceive),
                                    monObj);
            }
            catch (Exception ex)
            {
                var errArgs = new MonitorExceptionEventArgs(
                    ex, _socket, IPAddress, Port);

                OnErrorInternal(errArgs);
            }
        }


        public void Stop()
        {
            _continue = false;
            TryRelease(_socket);
        }


        private static NetworkInterface[] GetInterfaces()
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
            return validInterfaces.ToArray();
        }


        public static NetworkMonitor[] BindInterfaces()
        {
            var validInterfaces = GetInterfaces();
            var monitors = new List<NetworkMonitor>();
            foreach (var netInterface in validInterfaces) {
                var ipConfig = netInterface.GetIPProperties();
                foreach (var uni in ipConfig.UnicastAddresses) {
                    if (uni.Address.AddressFamily == AddressFamily.InterNetwork) {
                        var mon = new NetworkMonitor(uni.Address);
                        monitors.Add(mon);
                    }
                }
            }
            return monitors.ToArray();
        }


        public NetworkMonitor(IPAddress target)
        {
            IPAddress = target;
            Port = 0;
        }


        public NetworkMonitor(IPAddress target, int port)
        {
            IPAddress = target;
            Port = port;
        }


        public NetworkMonitor(IPEndPoint target)
        {
            IPAddress = target.Address;
            Port = target.Port;
        }


        ~NetworkMonitor() { Dispose(false); }
        public void Dispose() { Dispose(true); }
        private void Dispose(bool disposing)
        {
            if (disposing) {
                GC.SuppressFinalize(this);
            }
            TryRelease(_socket);
        }
    }
}
