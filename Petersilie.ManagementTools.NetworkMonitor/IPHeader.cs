using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;


namespace Petersilie.ManagementTools.NetworkMonitor
{
    public abstract class IPHeader
    {
        internal static readonly Func<byte, byte> LNIBBLE = (b) 
            => (byte)(b & 0x0f);

        internal static readonly Func<byte, byte> HNIBBLE = (b) 
            => (byte)((b >> 4) & 0xff);


        public IPAddress SourceAddress      { get; internal set; }

        public IPAddress DestinationAddress { get; internal set; }


        public abstract HeaderVersion HeaderVersion { get; }


        public static IPHeader Parse(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            {
                byte b = (byte)mem.ReadByte();
                byte version = HNIBBLE(b);
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
