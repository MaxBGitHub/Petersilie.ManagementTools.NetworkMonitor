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
        public IPAddress SourceAddress      { get; internal set; }

        public IPAddress DestinationAddress { get; internal set; }


        public abstract IPVersion IPVersion { get; }


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
