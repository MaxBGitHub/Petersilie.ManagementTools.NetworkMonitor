using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    internal class IPHeader
    {
        public static IIPHeader Parse(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                byte b = reader.ReadByte();
                byte Version = b.HighNibble();
                if (Version == 4) {
                    return new IPv4Header(packet);
                }
                else if (Version == 6) {
                    return new IPv6Header(packet);
                }
                else {
                    return null;
                }
            }
        }
    }
}
