using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor.Header
{
    public class TCPHeader
    {
        public ushort SourcePort { get; }
        public ushort DestinationPort { get; }
        public uint SequenceNumber { get; }
        public uint AcknowledgmentNumber { get; }
        public byte DataOffset { get; }
        public byte Reserved { get; }
        public byte[] Flags { get; } = new byte[8];
        public bool CWR { get { return Flags[0] == 1; } }
        public bool ECE { get { return Flags[1] == 1; } }
        public bool URG { get { return Flags[2] == 1; } }
        public bool ACK { get { return Flags[3] == 1; } }
        public bool PSH { get { return Flags[4] == 1; } }
        public bool RST { get { return Flags[5] == 1; } }
        public bool SYN { get { return Flags[6] == 1; } }
        public bool FIN { get { return Flags[7] == 1; } }
        public ushort Window { get; }
        public ushort Checksum { get; }
        public ushort UrgentPointer { get; }
        public byte[] Options { get; }
        public byte[] Data { get; }


        public TCPHeader(byte[] packet)
        {
            byte b;
            byte[] buffer;

            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                SourcePort = reader.ReadUInt16();
                DestinationPort = reader.ReadUInt16();

                SequenceNumber = reader.ReadUInt32();

                AcknowledgmentNumber = reader.ReadUInt32();

                b = reader.ReadByte();
                DataOffset = b.HighNibble();
                Reserved = b.LowNibble();
                buffer = reader.ReadBytes(1);
                var bits = new System.Collections.BitArray(buffer);
                Flags[0] = (byte)(bits[7] ? 1 : 0);
                Flags[1] = (byte)(bits[6] ? 1 : 0);
                Flags[2] = (byte)(bits[5] ? 1 : 0);
                Flags[3] = (byte)(bits[4] ? 1 : 0);
                Flags[4] = (byte)(bits[3] ? 1 : 0);
                Flags[5] = (byte)(bits[2] ? 1 : 0);
                Flags[6] = (byte)(bits[1] ? 1 : 0);
                Flags[7] = (byte)(bits[0] ? 1 : 0);

                Window = reader.ReadUInt16();

                Checksum = reader.ReadUInt16();
                UrgentPointer = reader.ReadUInt16();

                int payloadBegin = (DataOffset * 32) / 8;
                int optionLength = (int)(payloadBegin - mem.Position);
                int payloadLength = (int)(packet.Length - payloadBegin);

                Options = reader.ReadBytes(optionLength);
                Data = reader.ReadBytes(payloadLength);
            }
        }

    }
}
