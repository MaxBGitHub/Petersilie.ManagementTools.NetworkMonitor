using System;
using System.IO;
using System.Net;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /*  
    **   0           4           8            12         16           20          24           31
    **   |-------------------------------------------------------------------------------------|
    **   |  Version  |      Traffic class     |                  Flow label                    |
    **   |-------------------------------------------------------------------------------------|
    **   |                  Payload length               |        Next header     | Hop limit  |
    **   |-------------------------------------------------------------------------------------|
    **   |                                                                                     |
    **   |                                    Source address                                   |
    **   |                                                                                     |
    **   |                                                                                     |
    **   |-------------------------------------------------------------------------------------|
    **   |                                                                                     |
    **   |                                 Destination address                                 |
    **   |                                                                                     |
    **   |                                                                                     |
    **   |-------------------------------------------------------------------------------------|
    */
    public partial class IPv6Header : IIPHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        public byte[] Packet { get; }
        /// <summary>
        /// <see cref="Protocol.IP"/>
        /// </summary>
        public Protocol Protocol { get; } = Protocol.IP;
        /// <summary>
        /// Bits 0-3. IP-Version.
        /// </summary>
        public byte Version { get; }
        /// <summary>
        /// Bits 4-11. QoS (Bits 0-5 used for DSCP, Bits 6-7 used for ECN).
        /// </summary>
        public byte TrafficClass { get; }
        /// <summary>
        /// Bits 12-31. QoS and realtime applications.
        /// </summary>
        public int FlowLabel { get; }
        /// <summary>
        /// Length of data payload (without header bytes but 
        /// includes extension headers if any).
        /// </summary>
        public ushort PayloadLength { get; }
        /// <summary>
        /// Identifies the type of the next header 
        /// (either IPv6 extension header or other protocol).
        /// </summary>
        public byte NextHeader { get; }
        /// <summary>
        /// IPv6 migth contain an extended header.
        /// </summary>
        public Protocol NextProtocolOrHeader { get; } = Protocol.UNDEFINED;
        /// <summary>
        /// Maximum amount of hops between routers.
        /// </summary>
        public byte HopLimit { get; }
        /// <summary>
        /// Data payload containing extended header or other protocol.
        /// </summary>
        public byte[] Data { get; }
        /// <summary>
        /// IP version. Always 6 for IPv6 headers.
        /// </summary>
        public IPVersion IPVersion
        {
            get {
                return IPVersion.IPv6;
            }
        }
        /// <summary>
        /// Address of packet sender.
        /// </summary>
        public IPAddress SourceAddress { get; }
        /// <summary>
        /// Address of packet receiver.
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


            byte b;
            byte[] buffer;
            mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
                b = Version.AddLowNibble(TrafficClass.HighNibble());
                writer.Write(b);
                b = TrafficClass.LowNibble().AddLowNibble(FlowLabel.GetBits(0, 4).ToByte());
                writer.Write(b);
                writer.Write((ushort)FlowLabel.GetBits(4, 12).ToInt());
                writer.Write(PayloadLength);
                writer.Write(NextHeader);
                writer.Write(HopLimit);
                writer.Write(SourceAddress.GetAddressBytes());
                writer.Write(DestinationAddress.GetAddressBytes());
                writer.Write(Data);
                buffer = mem.ToArray();
            }
            return mem;
        }


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


        public IPv6Header(byte[] packet)
        {
            Packet = packet;

            byte b;
            byte[] buffer;
           
            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                // Get first byte.
                b = reader.ReadByte();
                // Get version from high nibble of first byte.
                Version = b.HighNibble();
                // Get traffic class from low nibble of first byte.
                TrafficClass = b.LowNibble();
                // Get next byte.
                b = reader.ReadByte();
                // Add high nibble of second byte to traffic class.
                TrafficClass += b.HighNibble();
                // Get flow label from low nibble of byte.
                FlowLabel = b.LowNibble();
                // Add next 2 bytes to flow label.
                FlowLabel += reader.ReadUInt16();
                // Get payload length from next to bytes.
                PayloadLength = reader.ReadUInt16();
                // Get next header from next byte.
                NextHeader = reader.ReadByte();
                Protocol p = Protocol.UNDEFINED;
                if (Enum.TryParse(NextHeader.ToString(), out p)) {
                    if (Enum.IsDefined(typeof(Protocol), p)) {
                        NextProtocolOrHeader = p;
                    } /* Check if protocol value is defined. */
                } /* Check if value can be parsed to Protocol enum. */
                
                // Get hop limit from next byte.
                HopLimit = reader.ReadByte();
                // Get source address from next 16 bytes.
                buffer = reader.ReadBytes(16);
                SourceAddress = new IPAddress(buffer);
                // Get destination address from next 16 bytes.
                buffer = reader.ReadBytes(16);
                DestinationAddress = new IPAddress(buffer);
                // Get extended header or next protocol.
                buffer = reader.ReadBytes(PayloadLength);
                Data = buffer;
            }
        }

    }
}
