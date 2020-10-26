using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;


namespace Petersilie.ManagementTools.NetworkMonitor.Header
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
        /// <summary>
        /// Bits 0-3. Always 4 in an IPv4 header.
        /// </summary>
        public byte Version { get; }
        /// <summary>
        /// Bits 4-7. IP header length.
        /// </summary>
        public byte IHL { get; }
        /// <summary>
        /// Length of header without options = (IHL * 32) / 8
        /// </summary>
        public int HeaderLength { get { return (IHL * 32) / 8; } }
        /// <summary>
        /// Bits 8-15. Type of service, used to set priority of datagram.
        /// </summary>
        public byte TOS { get; }
        /// <summary>
        /// Bits 16-31. Total length of packet in byte (includes header).
        /// </summary>
        public ushort TotalLength { get; }
        /// <summary>
        /// Bits 32-47. Used for reassembly on fragmented packets
        /// when combined with Flags and Fragment offset.
        /// </summary>
        public ushort Identification { get; }
        /// <summary>
        /// Bits 48-50.
        /// <para>Bit 0 = Reserved.</para>
        /// <para>Bit 1 = DF (Do not fragment).</para>
        /// <para>Bit 2 = MF (More fragments).</para>
        /// </summary>
        public byte[] Flags { get; } = new byte[3];
        /// <summary>
        /// Bit 0 of Flags.
        /// </summary>
        public byte FlagResered { get { return Flags[0]; } }
        /// <summary>
        /// Bit 1 of Flags.
        /// </summary>
        public bool FlagDoNotFragment { get { return Flags[1] == 1; } }
        /// <summary>
        /// Bit 2 of Flags.
        /// </summary>
        public bool FlagMoreFragments { get { return Flags[2] == 1; } }   
        /// <summary>
        /// True if all Flag bits are 0.
        /// Indicates that there are no more fragements remaining.
        /// </summary>
        public bool FlagNoMoreFragments { get { return (Flags[0] + Flags[1] + Flags[2]) == 0; } }
        /// <summary>
        /// Bits 51-63. Contains start position of fragment
        /// if packet is fragmented.
        /// </summary>
        public ushort FragmentOffset { get; }
        /// <summary>
        /// Bits 64-71. Time to live.
        /// </summary>
        public byte TTL { get; }
        /// <summary>
        /// Bits 72-80. Protocol contained in the data/payload section.
        /// </summary>
        public Protocol Protocol { get; } = Protocol.UNDEFINED;
        /// <summary>
        /// Bits 81-96. Checksum of header.
        /// </summary>
        public ushort HeaderChecksum { get; }
        /// <summary>
        /// Bits 128-?. Possible Options are:
        /// <para>Strict Routing: Option contains whole path 
        /// that packet needs to go.</para>
        /// <para>Free Routing: Option contains list of routers
        /// that the packet is not allowed to miss.</para>
        /// <para>Record Router: Records whole route.</para>
        /// <para>Time Stamp: Timestamp.</para>
        /// <para>Security: Defines </para>
        /// </summary>
        public byte[] OptionsAndPadding { get; } = new byte[0];
        /// <summary>
        /// Bits ?-?. Contains data of next protocol
        /// </summary>
        public byte[] Data { get; } = new byte[0];

        public override IPVersion IPVersion { get { return IPVersion.IPv4; } }

        
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
                b = reader.ReadByte();
                Version = b.HighNibble();
                IHL = b.LowNibble();
                TOS = reader.ReadByte();
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
