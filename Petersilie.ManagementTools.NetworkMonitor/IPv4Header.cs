using System;
using System.IO;
using System.Net;
using System.Linq;


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
    public partial class IPv4Header : IIPHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        public byte[] Packet { get; }
        /// <summary>
        /// Bits 72-79. Next protocol within data payload.
        /// </summary>
        public Protocol Protocol { get; } = Protocol.IP;
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
        public int HeaderLength
        {
            get {
                return (IHL * 32) / 8;
            }
        }
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
        public bool[] Flags { get; } = new bool[3];
        /// <summary>
        /// Bit 0 of Flags.
        /// </summary>
        public bool FlagResered { get { return Flags[0]; } }
        /// <summary>
        /// Bit 1 of Flags.
        /// </summary>
        public bool FlagDoNotFragment { get { return Flags[1]; } }
        /// <summary>
        /// Bit 2 of Flags.
        /// </summary>
        public bool FlagMoreFragments { get { return Flags[2]; } }
        /// <summary>
        /// True if all Flag bits are 0.
        /// Indicates that there are no more fragements remaining.
        /// </summary>
        public bool FlagNoMoreFragments { get { return Flags[0] && 
                                                       Flags[1] && 
                                                       Flags[2]; } }
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
        /// Bits 81-96. Checksum of header.
        /// </summary>
        public ushort HeaderChecksum { get; }
        /// <summary>
        /// Bits 160-?. Possible Options are:
        /// <para>Strict Routing: Option contains whole path 
        /// that packet needs to go.</para>
        /// <para>Free Routing: Option contains list of routers
        /// that the packet is not allowed to miss.</para>
        /// <para>Record Router: Records whole route.</para>
        /// <para>Time Stamp: Timestamp.</para>
        /// <para>Security: Defines how secret packet is.</para>
        /// </summary>
        public byte[] OptionsAndPadding { get; } = new byte[0];
        /// <summary>
        /// Bits ?-?. Contains data of next protocol.
        /// </summary>
        public byte[] Data { get; } = new byte[0];
        /// <summary>
        /// IP Version 4.
        /// </summary>
        public IPVersion IPVersion
        {
            get {
                return IPVersion.IPv4;
            }
        }
        /// <summary>
        /// Bits 96-127. Address of packet sender.
        /// </summary>
        public IPAddress SourceAddress { get; }
        /// <summary>
        /// Bits 128-159. Address of packet receiver.
        /// </summary>
        public IPAddress DestinationAddress { get; }


        public Stream ToStream()
        {
            MemoryStream mem = null;

            if (null != Packet) {
                if (0 <= Packet.Length) {
                    mem = new MemoryStream(Packet);
                    mem.Position = 0;
                    return mem;
                } /* Packet has data. */         
            } /* Packet is not null. */

            mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
                // Combine Versiona and IHL nibbles and write complete byte.
                writer.Write(Version.AddLowNibble(IHL));
                // Write TOS byte.
                writer.Write(TOS);
                // Write total length bytes.
                writer.Write(TotalLength);
                // Write identification bytes.
                writer.Write(Identification);

                // Create 2 byte long data type and assign flags to it.
                ushort s = (ushort)Flags.ToByte();
                // Combine and write the flags and the fragment offset.
                writer.Write((ushort)(s + FragmentOffset));
                // Write ttl byte.
                writer.Write(TTL);
                // Write protocl byte.
                writer.Write((byte)Protocol);
                // Write checksum bytes.
                writer.Write(HeaderChecksum);
                // Write source address bytes.
                writer.Write(SourceAddress.GetAddressBytes());
                // Write destination address bytes.
                writer.Write(DestinationAddress.GetAddressBytes());
                // Write options and padding bytes.
                writer.Write(OptionsAndPadding);
                // Write data payload bytes.
                writer.Write(Data);                
            }
            return mem;
        }


        /// <summary>
        /// Converts the IPv4 header back to a raw byte stream.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByte()
        {
            if (null != Packet) {
                if (0 <= Packet.Length) {
                    // Return packet instead of parsing data.
                    return Packet;
                } /* Packet has data. */         
            } /* Packet is not null. */

            MemoryStream mem = ToStream() as MemoryStream;
            return mem.ToArray();
        }


        
        public IPv4Header(byte[] packet)
        {
            byte b;
            byte[] buffer;

            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {                
                // Read first byte.
                b = reader.ReadByte();
                // Get version from high nibble of first byte.
                Version = b.HighNibble();
                // Get IHL from low nibble of first byte.
                IHL = b.LowNibble();
                // Get TOS from next byte.
                TOS = reader.ReadByte();
                // Get total length from next 2 bytes.
                TotalLength = reader.ReadUInt16();
                // Get identification from next 2 bytes.
                Identification = reader.ReadUInt16();
                // Get next byte.
                b = reader.ReadByte();
                // Reserved flag in Bit 0.
                Flags[0] = b.GetBit(7);
                // Do not fragment flag in Bit 1.
                Flags[1] = b.GetBit(6);
                // More fragments flag in Bit 2.
                Flags[2] = b.GetBit(5);
                // Get next byte.
                b = reader.ReadByte();
                /* Fragment offset has 13-Bit.
                ** Get remaining Bits from previous byte
                ** and add Bits of current byte. */ 
                FragmentOffset = (ushort)b.GetBits(0, 5)
                                          .AddBits(b)
                                          .ToInt();
                // Get ttl from next byte.
                TTL = reader.ReadByte();
                // Get next byte.
                b = reader.ReadByte();
                // Try to parse byte into Protocol enum.
                Protocol p = Protocol.UNDEFINED;
                if (Enum.TryParse(b.ToString(), out p)) {
                    if (Enum.IsDefined(typeof(Protocol), p)) {
                        Protocol = p;
                    } /* Check if enum value is defined. */
                } /* Check if value can be parsed to enum. */

                // Get header checksum from next 2 bytes.
                HeaderChecksum = reader.ReadUInt16();
                // Read next 4 bytes.
                buffer = reader.ReadBytes(4);
                // Parse bytes to IP address.
                SourceAddress = new IPAddress(buffer);
                // Read next 4 bytes.
                buffer = reader.ReadBytes(4);
                // Parse bytes to IP address.
                DestinationAddress = new IPAddress(buffer);
                // Calculate the length of the options and padding.
                int optionLength = (IHL * 4) - 20;
                // Load n bytes into buffer array.
                buffer = reader.ReadBytes(optionLength);
                // Set options and padding.
                OptionsAndPadding = buffer;
                // Calculate length of data payload.
                int dataLength = packet.Length - (int)mem.Position;
                // Load n bytes into buffer array.
                buffer = reader.ReadBytes(dataLength);
                // Set data payload.
                Data = buffer;
            }
        }
    }
}
