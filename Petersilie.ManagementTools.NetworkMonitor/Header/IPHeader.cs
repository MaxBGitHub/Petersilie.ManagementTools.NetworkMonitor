using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;


namespace Petersilie.ManagementTools.NetworkMonitor.Header
{
    public abstract class IPHeader
    {
        /// <summary>
        /// When overriden in a class, contains the source
        /// IP address which is contained in the IP header bytes.
        /// </summary>
        public IPAddress SourceAddress { get; internal set; }
        /// <summary>
        /// When overriden in a class, contains the destination
        /// IP address which is contained in the IP header bytes.
        /// </summary>
        public IPAddress DestinationAddress { get; internal set; }

        /// <summary>
        /// When overriden in a class, either IPv4 or IPv6.
        /// </summary>
        public abstract IPVersion IPVersion { get; }


        /// <summary>
        /// Parse the raw byte array and try to
        /// build either a IPv4 or IPv6 header
        /// out of. Bits 0-4 contain the IP headers Version.
        /// </summary>
        /// <param name="packet">Raw IP header byte data.</param>
        /// <returns></returns>
        public static IPHeader Parse(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            {
                byte b = (byte)mem.ReadByte();
                byte version = b.HighNibble();
                if (version == 4) {
                    return new IPv4Header(packet);
                }
                else if (version == 6) {
                    return new IPv6Header(packet);
                }
                else {
                    return null;
                }
            }
        }
    }
}
