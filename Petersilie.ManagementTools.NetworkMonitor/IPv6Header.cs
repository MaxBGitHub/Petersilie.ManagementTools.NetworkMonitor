using System.IO;
using System.Net;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public partial class IPv6Header : IIPHeader
    {
        public byte Version { get; }

        public byte TrafficClass { get; }

        public int FlowLabel { get; }

        public ushort PayloadLength { get; }

        public byte NextHeader { get; }

        public byte HopLimit { get; }

        public IPVersion IPVersion
        {
            get {
                return IPVersion.IPv6;
            }
        }

        public IPAddress SourceAddress { get; }

        public IPAddress DestinationAddress { get; }


        public IPv6Header(byte[] packet)
        {
            byte b;
            byte[] buffer;

            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                b = reader.ReadByte();
                Version = b.HighNibble();

                TrafficClass = b.LowNibble();
                b = reader.ReadByte();
                TrafficClass += b.HighNibble();

                FlowLabel = b.LowNibble();
                FlowLabel += reader.ReadUInt16();

                PayloadLength = reader.ReadUInt16();

                NextHeader = reader.ReadByte();

                HopLimit = reader.ReadByte();

                buffer = reader.ReadBytes(16);
                SourceAddress = new IPAddress(buffer);

                buffer = reader.ReadBytes(16);
                DestinationAddress = new IPAddress(buffer);
            }
        }

    }
}
