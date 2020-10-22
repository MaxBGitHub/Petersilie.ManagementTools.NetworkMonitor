using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace Petersilie.ManagementTools.NetworkMonitor
{
    /*  
    **   0           4           8            12         16           20          24           31
    **   |-------------------------------------------------------------------------------------|
    **   |  Version  |    IHL    |           TOS         |             Total length            |
    **   |-------------------------------------------------------------------------------------|
    **   |                Identification                 |   Flags   |     Fragment offset     |
    **   |-------------------------------------------------------------------------------------|
    **   |           TTL         |        Protocol       |           Header checksum           |
    **   |-------------------------------------------------------------------------------------|
    **   |                                   Source address                                    |
    **   |-------------------------------------------------------------------------------------|
    **   |                                 Destination address                                 |
    **   |-------------------------------------------------------------------------------------|
    **   |                                                                                     |
    **   |                                       Data                                          |
    **   |                                                                                     |
    **   |-------------------------------------------------------------------------------------| 
    */
    public partial class IPv4Header : IPHeader
    {
        public byte Version { get; }

        public byte IHL { get; }

        public int HeaderLength { get { return (IHL * 32) / 8; } }

        public byte TOS { get; }

        public ushort TotalLength { get; }

        public ushort Identification { get; }

        public byte[] Flags { get; } = new byte[3];

        public byte FlagResered { get { return Flags[0]; } }

        public bool FlagDoNotFragment { get { return Flags[1] == 1; } }

        public bool FlagMoreFragments { get { return Flags[2] == 1; } }   
        
        public bool FlagNoMoreFragments { get { return (Flags[0] + Flags[1] + Flags[2]) == 0; } }

        public ushort FragmentOffset { get; }

        public byte TTL { get; }

        public Protocol Protocol { get; } = Protocol.UNDEFINED;

        public ushort HeaderChecksum { get; }

        public byte[] OptionsAndPadding { get; } = new byte[0];

        public byte[] Data { get; } = new byte[0];

        public override HeaderVersion HeaderVersion { get { return HeaderVersion.IPv4; } }

        
        public IPv4Header(byte[] packet)
        {            
            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                byte b;
                byte[] buffer;

                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |  Version  |    IHL    |           TOS         |             Total length            |
                **  |-------------------------------------------------------------------------------------| 
                */
                b       = reader.ReadByte();
                Version = HNIBBLE(b);
                IHL     = LNIBBLE(b);
                TOS     = reader.ReadByte();
                TotalLength = reader.ReadUInt16();


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |                Identification                 | Flags     |     Fragment offset     |
                **  |-------------------------------------------------------------------------------------|
                */
                Identification = reader.ReadUInt16();
                var bits = new System.Collections.BitArray(new byte[] { reader.ReadByte() });
                Flags[0] = (byte)(bits.Get(5) ? 1 : 0);
                Flags[1] = (byte)(bits.Get(6) ? 1 : 0);
                Flags[2] = (byte)(bits.Get(7) ? 1 : 0);
                bits.Set(5, false);
                bits.Set(6, false);
                bits.Set(7, false);
                buffer = new byte[1];
                bits.CopyTo(buffer, 0);
                FragmentOffset = buffer[0];
                FragmentOffset += reader.ReadByte();


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |           TTL         |        Protocol       |           Header checksum           |
                **  |-------------------------------------------------------------------------------------|
                */
                TTL = reader.ReadByte();
                b = reader.ReadByte();
                Protocol p;
                if (Enum.TryParse(b.ToString(), out p)) {
                    if (Enum.IsDefined(typeof(Protocol), p)) {
                        Protocol = p;
                    } else {
                        Protocol = Protocol.UNDEFINED;
                    }
                } else {
                    Protocol = Protocol.UNDEFINED;
                }
                HeaderChecksum = reader.ReadUInt16();


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |                                   Source address                                    |
                **  |-------------------------------------------------------------------------------------|
                */
                buffer = reader.ReadBytes(4);
                SourceAddress = new IPAddress(buffer);


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |                                 Destination address                                 |
                **  |-------------------------------------------------------------------------------------|
                */
                buffer = reader.ReadBytes(4);
                DestinationAddress = new IPAddress(buffer);


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |                                 Options and padding                                 |
                **  |-------------------------------------------------------------------------------------|
                */
                int optionLength = (IHL * 4) - 20;
                buffer = reader.ReadBytes(optionLength);
                OptionsAndPadding = buffer;


                /*  0           4           8            12         16           20          24           31
                **  |-------------------------------------------------------------------------------------|
                **  |                                                                                     |
                **  |                                       Data                                          |
                **  |                                                                                     |
                **  |-------------------------------------------------------------------------------------| 
                */
                int dataLength = packet.Length - (int)mem.Position;
                buffer = reader.ReadBytes(dataLength);
                Data = buffer;
            }
        }
    }
}
