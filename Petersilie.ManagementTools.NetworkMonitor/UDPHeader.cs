using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public class UDPHeader
    {
        public ushort SourcePort { get; }
        public ushort DestinationPort { get; }
        public ushort Length { get; }
        public ushort Checksum { get; }
        public byte[] Data { get; }

        public UDPHeader(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                SourcePort = reader.ReadUInt16();
                DestinationPort = reader.ReadUInt16();
                Length = reader.ReadUInt16();
                Checksum = reader.ReadUInt16();

                int dataLength = (int)(packet.Length - mem.Position);
                Data = reader.ReadBytes(dataLength);
            }
        }
    }
}
