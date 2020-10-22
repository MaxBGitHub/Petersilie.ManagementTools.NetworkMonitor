using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public partial class IPv6Header : IPHeader
    {
        public byte Version { get; }

        public byte TrafficClass { get; }

        public int FlowLabel { get; }

        public ushort PayloadLength { get; }

        public byte NextHeader { get; }

        public byte HopLimit { get; }

        public override HeaderVersion HeaderVersion
        {
            get {
                return HeaderVersion.IPv6;
            }
        }


        public IPv6Header(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            {
                byte b = (byte)mem.ReadByte();
                Version = HNIBBLE(b);

                TrafficClass = LNIBBLE(b);
                b = (byte)mem.ReadByte();
                TrafficClass += HNIBBLE(b);

                FlowLabel = LNIBBLE(b);
                byte[] buffer = new byte[2];
                mem.Read(buffer, 0, 2);
                FlowLabel += BitConverter.ToUInt16(buffer, 0);

                buffer = new byte[2];
                mem.Read(buffer, 0, 2);
                PayloadLength = BitConverter.ToUInt16(buffer, 0);

                b = (byte)mem.ReadByte();
                NextHeader = b;

                b = (byte)mem.ReadByte();
                HopLimit = b;

                buffer = new byte[16];
                mem.Read(buffer, 0, 16);
                SourceAddress = new IPAddress(buffer);

                buffer = new byte[16];
                mem.Read(buffer, 0, 16);
                DestinationAddress = new IPAddress(buffer);                
            }
        }

    }
}
