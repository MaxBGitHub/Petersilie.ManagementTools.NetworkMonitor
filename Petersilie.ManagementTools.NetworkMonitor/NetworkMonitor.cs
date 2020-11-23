using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;


namespace Petersilie.ManagementTools.NetworkMonitor
{    
    public class NetworkMonitor : IDisposable
    {
        public const int DEFAULT_BUFFERSIZE = 0x1000;

        /// <summary>
        /// Amount of bytes that the socket can receive.
        /// </summary>
        public int BufferSize { get; set; } = DEFAULT_BUFFERSIZE;

        /// <summary>
        /// Socket bound IP address.
        /// </summary>
        public IPAddress IPAddress { get; }

        /// <summary>
        /// Socket bound port.
        /// </summary>
        public int Port { get; }


        private event EventHandler<PacketErrorEventArgs> onError;
        /// <summary>
        /// Occurs whenever the monitor runs into an exception or error.
        /// </summary>
        public event EventHandler<PacketErrorEventArgs> OnError
        {
            add {
                onError += value;
            }
            remove {
                onError -= value;
            }
        }

        protected virtual void OnErrorInternal(PacketErrorEventArgs e)
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
            if ( null == monObj ) {
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
                        // Assign SocketError.NoData.
                        err = SocketError.NoData;
                    } /* Check if we ran into any errors. */

                    /* Create event args with no header object,
                    ** the IP address, the port and the SocketError. */
                    ipArgs = new PacketEventArgs(   null, 
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

                    /* Create event args with header object,
                    ** IP address, port and no error. */
                    ipArgs = new PacketEventArgs(bytesReceived,
                                                 IPAddress,
                                                 Port);                        
                } /* Data received. */

                // Raise event.
                OnPacketReceived(ipArgs);
                // Create new SocketStateObject instance.
                monObj = new SocketStateObject(socket, BufferSize);
                // Continue receiving raw packets.
                socket.BeginReceive(monObj.Data, 0,
                                    BufferSize,
                                    SocketFlags.None,
                                    new AsyncCallback(OnReceive),
                                    monObj);
            }
            catch (Exception ex) {                
                // Create new error event args.
                var errArgs = new PacketErrorEventArgs(
                    ex, _socket, IPAddress, Port);
                // Raise error event.
                OnErrorInternal(errArgs);
            }
        }


        public void Begin()
        {
            try
            {
                // Init socket for raw, IP based network listening.
                _socket = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Raw,
                                     ProtocolType.IP);
                
                // Bind socket to IP and port.
                _socket.Bind(new IPEndPoint(IPAddress, Port));

                /* Configure socket to listen on all IP packets
                ** and include headers. */
                _socket.SetSocketOption(SocketOptionLevel.IP,
                                        SocketOptionName.HeaderIncluded,
                                        true);

                // State object for async callback.
                SocketStateObject monObj = new SocketStateObject(_socket, 
                                                                BufferSize);
                
                // Set size of receive data buffer of socket.
                _socket.ReceiveBufferSize = BufferSize;
                
                /* Configure IO control to receive all incoming
                ** aswell as all out going data. */
                _socket.IOControl(IOControlCode.ReceiveAll,
                                RCVALL_ON, 
                                RCVALL_ON);

                /* Start receiving data from clients.
                ** Store connection data in monObj.Data.
                ** Set data offset to 0 and use user defined
                ** buffer size. Do not pass any socket flags.
                ** Assign async callback and pass monObj as
                ** State object of the connection. */
                _socket.BeginReceive(monObj.Data,
                                    0, BufferSize,
                                    SocketFlags.None,
                                    new AsyncCallback(OnReceive),
                                    monObj);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        /// <summary>
        /// Stops the NetworkMonitor from receiving any data.
        /// </summary>
        public void Stop()
        {
            _continue = false;
            TryRelease(_socket);
        }


        /* Returns an array of valid network interfaces.
        ** Used to create a NetworkMonitor object for 
        ** all up and running network interfaces.
        ** Valid interfaces are those that are not a
        ** loopback or tunnel interface/adapter and 
        ** are up and running and not a virtual ethernet
        ** interface/adapter. */
        private static NetworkInterface[] GetInterfaces()
        {
            // Unallowed interface types.
            var invalidInterfaces = new NetworkInterfaceType[2] {
                NetworkInterfaceType.Loopback,
                NetworkInterfaceType.Tunnel
            };

            // Store all interfaces.
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            // Stores all valid interfaces.
            var validInterfaces = new List<NetworkInterface>();
            foreach (var intf in interfaces) {
                if (!(invalidInterfaces.Contains(intf.NetworkInterfaceType))) {
                    if (OperationalStatus.Up == intf.OperationalStatus 
                        && !(intf.Name.StartsWith("vEthernet")))
                    {
                        validInterfaces.Add(intf);
                    } /* Check if running and not virtual ethernet. */
                } /* Check for valid interface type. */
            } /* Loop through available interfaces. */
            return validInterfaces.ToArray();
        }


        /// <summary>
        /// Creates a NetworkMonitor instance for each 
        /// valid network interface.
        /// </summary>
        /// <returns>Returns an array of NetworkMonitor instances.</returns>
        public static NetworkMonitor[] BindInterfaces()
        {
            // Get all up and running interfaces.
            var validInterfaces = GetInterfaces();
            // List for storing NetworkMonitor objects.
            var monitors = new List<NetworkMonitor>();

            foreach (var netInterface in validInterfaces) {
                var ipConfig = netInterface.GetIPProperties();
                foreach (var uni in ipConfig.UnicastAddresses) {
                    if (uni.Address.AddressFamily 
                        == AddressFamily.InterNetwork)
                    {
                        var mon = new NetworkMonitor(uni.Address);
                        monitors.Add(mon);
                    } /* Check if IPv4. */
                } /* Loop through unicast addresses. */
            } /* Loop through all valid interfaces. */

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
