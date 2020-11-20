using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace Petersilie.ManagementTools.NetworkMonitor
{    
    public class NetworkMonitor : IDisposable
    {
        /// <summary>
        /// Runtime duration of the NetworkMonitor.
        /// </summary>
        public TimeSpan UpTime
        {
            get
            {
                if (_continue) {
                    return DateTime.Now - _startedAt;
                } else {
                    return _endedAt - _startedAt;
                }
            }
        }

        // Start time of the NetworkMonitor.
        private DateTime _startedAt;
        // Time the NetworkMonitor ended.
        private DateTime _endedAt;

        /// <summary>
        /// Socket bound IP address.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Socket bound port.
        /// </summary>
        public int Port { get; }


        private event EventHandler<MonitorExceptionEventArgs> onError;
        /// <summary>
        /// Occurs whenever the monitor runs into an exception or error.
        /// </summary>
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


        private event EventHandler<PacketEventArgs> onPacketReceived;
        /// <summary>
        /// Occurs whenever the monitor received a IP packet.
        /// </summary>
        public event EventHandler<PacketEventArgs> PacketReceived
        {
            add {
                onPacketReceived += value;
            }
            remove {
                onPacketReceived -= value;
            }
        }

        protected virtual void OnPacketReceived(PacketEventArgs ipArgs)
        {
            onPacketReceived?.Invoke(this, ipArgs);
        }


        // Stops execution if set to FALSE.
        private bool _continue = true;
        // Socket used for monitoring.
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
        static readonly byte[] RCVALL_ON = new byte[4] {
            1,
            0,
            0,
            0
        };


        // Tries to release the specified socket object.
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


        /* Async callback for Socket.BeginReceive.
        ** Stops when _continue is set to FALSE.
        ** IAsyncResult.AsyncState contains a SocketStateObject.
        ** Raises the PacketReceived event after parsing a valid packet.
        ** Raises the OnError event if anything failes. */
        private void OnReceive(IAsyncResult ar)
        {
            /* Get SocketStateObject instance stored in 
            ** the IAsyncResult.AsyncState object member. */
            SocketStateObject monObj = ar.AsyncState as SocketStateObject;
            if (null == monObj) {
                _continue = false;
                return;
            } /* Check if SocketStateObject is null. */
            
            if ( !_continue ) {
                TryRelease(monObj.Socket);
                return;
            } /* Continue receiving? */

            try
            {
                // Get socket from SocketStateObject.
                Socket socket = monObj.Socket;
                if (null == socket) {
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                    return;
                } /* Check if received Socket is null. */

                // Stores any socket error.
                SocketError err;
                // Amount of bytes received from Socket.
                int nReceived = socket.EndReceive(ar, out err);
                // Allocate byte array to store received bytes.
                byte[] bytesReceived = new byte[nReceived];
                // Event args for PacketEventArgs.
                PacketEventArgs ipArgs = null;

                if ( 1 > nReceived ) {
                    if (SocketError.Success == err) {
                        // Assing SocketError.NoData.
                        err = SocketError.NoData;
                    } /* Check if we ran into any errors. */

                    /* Create event args with no header object,
                    ** the IP address, the port and the SocketError. */
                    ipArgs = new PacketEventArgs( null, 
                                                    IPAddress, 
                                                    Port, 
                                                    err);
                } /* No data received. */
                else {
                    // Copy received bytes to array.
                    Buffer.BlockCopy(monObj.Data, 
                                     0, 
                                     bytesReceived, 
                                     0, 
                                     nReceived);
                    // Parse bytes into IP header.
                    var header = IPHeaderUtil.Parse(bytesReceived);                    
                    /* Create event args with header object,
                    ** IP address, port and no error. */
                    ipArgs = new PacketEventArgs(bytesReceived,
                                                 IPAddress,
                                                 Port);                        
                } /* Data received. */

                // Raise event.
                OnPacketReceived(ipArgs);
                // Create new SocketStateObject instance.
                monObj = new SocketStateObject(socket);
                // Continue receiving raw packets.
                socket.BeginReceive(monObj.Data, 0,
                                    SocketStateObject.BUFFER_SIZE,
                                    SocketFlags.None,
                                    new AsyncCallback(OnReceive),
                                    monObj);
            }
            catch (Exception ex) {                
                // Create new error event args.
                var errArgs = new MonitorExceptionEventArgs(
                    ex, _socket, IPAddress, Port);
                // Raise error event.
                OnErrorInternal(errArgs);
            }
        }


        public void Begin()
        {
            _startedAt = DateTime.Now;
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
                throw new Exception(ex.Message);
            }
        }


        public void Stop()
        {
            _endedAt = DateTime.Now;
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
            _endedAt = DateTime.Now;
            if (disposing) {
                GC.SuppressFinalize(this);
            }
            TryRelease(_socket);
        }
    }
}
