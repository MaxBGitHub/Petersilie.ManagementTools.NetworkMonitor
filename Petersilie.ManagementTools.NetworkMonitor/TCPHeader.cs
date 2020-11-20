using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /*  
    **   0             4             8               12              16                20                24                 31
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                      Source port                          |                    Destination port                  |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                                                   Sequence number                                                |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                                                Acknowledgment number                                             |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |     Data    |             | C | E | U | A | P | R | S | F |                                                      |
    **   |    offset   |   Reserved  | W | C | R | C | S | S | Y | I |                        Window                        |
    **   |             |             | R | E | G | K | H | T | N | N |                                                      |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                       Cheksum                             |                    Urgent pointer                    |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                                                        Options                                                   |
    **   |                                              (0 or multiple 32-Bit WORDS)                                        |
    **   |------------------------------------------------------------------------------------------------------------------|
    **   |                                                         Data                                                     |
    **   |                                                      (Payload)                                                   |
    **   |------------------------------------------------------------------------------------------------------------------|
    */
    public class TCPHeader : IHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        public byte[] Packet { get; }
        /// <summary>
        /// <see cref="Protocol.TCP"/>
        /// </summary>
        public Protocol Protocol { get; } = Protocol.TCP;
        /// <summary>
        /// Bits 0-15. Identifies the sending port.
        /// </summary>
        public ushort SourcePort { get; }
        /// <summary>
        /// Bits 16-31. Identifies the receiving port.
        /// </summary>
        public ushort DestinationPort { get; }
        /// <summary>
        /// Bits 32-63.
        /// <para>
        /// SYN flag is set (1): Sequence number of the actual first data byte 
        /// and acknowledged number in the corresponding ACK are then this 
        /// sequence number plus 1.
        /// </para>
        /// <para>
        /// SYN flag is clear (0): Accumulated sequence number of the first 
        /// data byte of this segment for the current session.
        /// </para>
        /// </summary>
        public uint SequenceNumber { get; }
        /// <summary>
        /// Bits 64-95.
        /// If the ACK flag is set then the value of this field is the next 
        /// squence number that the sender of the ACK is expecting.
        /// <para>
        /// This acknowledges receipt of all prior bytes (if any).
        /// </para>
        /// <para>
        /// The first ACK sent by each end acknowledges the other end's initial
        /// sequence number itself, but no data.
        /// </para>
        /// </summary>
        public uint AcknowledgmentNumber { get; }
        /// <summary>
        /// Bits 96-99. 
        /// Specifies the size of the TCP header in 32-Bit words.
        /// <para>
        /// The minimum size header is 5 words and the maximum is 15 words thus
        /// giving the minimum size of 20 bytes and maximum of 60 bytes, 
        /// allowing for up to 40 bytes of options in the header.
        /// </para>
        /// </summary>
        public byte DataOffset { get; }
        /// <summary>
        /// Bits 100-103.
        /// For furture use and should be set to zero.
        /// </summary>
        public byte Reserved { get; }
        /// <summary>
        /// Bits 104-111.
        /// Contains 8 1-Bit flags (control bits).
        /// <para>
        /// CWR: Congestion window reduced (CWR) flag is set by the 
        /// sending host to indicate that it received a TCP segment with 
        /// the ECE flag set and had responded in congestion control 
        /// mechanism.
        /// </para>
        /// <para>ECE: ECN-Echo has a dual role, depending on the value of 
        /// the SYN flag. If SYN flag is set (1), it indicates that the TCP
        /// peer is ECN capable. If the SYN flag is clear (0), it indicates
        /// that a packet with Congestion Experienced flag set (ECN=11) in the
        /// IP header was received during normal transmission (serves as
        /// indication of network congestion to TCP sender).
        /// </para>
        /// <para>
        /// URG: Indicates that the Urgent pointer field is significant.
        /// </para>
        /// <para>
        /// ACK: Indicates that the Acknowledgment field is significant.
        /// </para>
        /// <para>
        /// PSH: Push function. Asks to push the buffered data to the 
        /// receiving application.
        /// </para>
        /// <para>
        /// RST: Reset the connection.
        /// </para>
        /// <para>
        /// SYN: Synchronize sequence numbers.
        /// Only the first packet sent from each end should have this flag set.
        /// </para>
        /// <para>
        /// FIN: Last packet from sender.
        /// </para>
        /// </summary>
        public bool[] Flags { get; } = new bool[8];
        /// <summary>
        /// Congestion window reduced (CWR) flag is set by the 
        /// sending host to indicate that it received a TCP segment with 
        /// the ECE flag set and had responded in congestion control mechanism.
        /// </summary>
        public bool CWR { get { return Flags[0]; } }
        /// <summary>
        /// ECN-Echo has a dual role, depending on the value of 
        /// the SYN flag. If SYN flag is set (1), it indicates that the TCP
        /// peer is ECN capable. If the SYN flag is clear (0), it indicates
        /// that a packet with Congestion Experienced flag set (ECN=11) in the
        /// IP header was received during normal transmission (serves as
        /// indication of network congestion to TCP sender).
        /// </summary>
        public bool ECE { get { return Flags[1]; } }
        /// <summary>
        /// Indicates that the Urgent pointer field is significant.
        /// </summary>
        public bool URG { get { return Flags[2]; } }
        /// <summary>
        /// Indicates that the Acknowledgment field is significant.
        /// </summary>
        public bool ACK { get { return Flags[3]; } }
        /// <summary>
        /// Push function. Asks to push the buffered data to the
        /// receiving application.
        /// </summary>
        public bool PSH { get { return Flags[4]; } }
        /// <summary>
        /// Reset the connection.
        /// </summary>
        public bool RST { get { return Flags[5]; } }
        /// <summary>
        /// Synchronize sequence numbers.
        /// Only the first packet sent from each end should have this flag set.
        /// </summary>
        public bool SYN { get { return Flags[6]; } }
        /// <summary>
        /// Last packet from sender.
        /// </summary>
        public bool FIN { get { return Flags[7]; } }
        /// <summary>
        /// Bits 112-127. The size of the receive window, which specifies the
        /// number of window size units that the sender of this segment is 
        /// currently willing to receive.
        /// </summary>
        public ushort Window { get; }
        /// <summary>
        /// Bits 128-142. The 16-Bit checksum field is used for error-checking
        /// of the TCP header, the payload and an IP pseudo-header.
        /// The pseudo-header consists of the source IP address, the 
        /// destination IP address, the protocol number for the TCP protocol
        /// (6) and the length of the TCP headers and payload (in bytes).
        /// </summary>
        public ushort Checksum { get; }
        /// <summary>
        /// Bits 143-158. If the URG flag is set, then this 16-Bit field is an
        /// offset from the sequence number indicating the last urgent data 
        /// byte.
        /// </summary>
        public ushort UrgentPointer { get; }
        /// <summary>
        /// Bits 159-?. Length of this field is determined by the data offset 
        /// field. OptionsAndPadding have up to three fields:
        /// <para>
        /// Option-Kind (1 byte): Indicates the type of option and is the only 
        /// field that is not optional.
        /// </para>
        /// <para>
        /// Option-Length (1 byte): Indicates the total length of the option.
        /// </para>
        /// <para>Option-Data (n bytes): Additional data.</para>
        /// <para>Padding is added if necessary and ensures that the TCP header
        /// begins and ends on a 32-Bit boundry. Padding is composed of zeros.
        /// </para>
        /// </summary>
        public byte[] OptionsAndPadding { get; }
        /// <summary>
        /// Bits ?-?. Data payload. 
        /// </summary>
        public byte[] Data { get; }



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

            using (var writer = new BinaryWriter(mem))
            {
                writer.Write(SourcePort);
                writer.Write(DestinationPort);
                writer.Write(SequenceNumber);
                writer.Write(AcknowledgmentNumber);
                byte b = DataOffset.AddLowNibble(Reserved);
                writer.Write(b);
                writer.Write(Flags.ToByte());
                writer.Write(Window);
                writer.Write(Checksum);
                writer.Write(UrgentPointer);
                writer.Write(OptionsAndPadding);
                writer.Write(Data);
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


        public TCPHeader(byte[] packet)
        {
            Packet = packet;

            byte b;

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

                b = reader.ReadByte();
                Flags = b.GetBits();

                Window = reader.ReadUInt16();

                Checksum = reader.ReadUInt16();
                UrgentPointer = reader.ReadUInt16();

                int payloadBegin = (DataOffset * 32) / 8;
                int optionLength = (int)(payloadBegin - mem.Position);
                int payloadLength = (int)(packet.Length - payloadBegin);

                OptionsAndPadding = reader.ReadBytes(optionLength);
                Data = reader.ReadBytes(payloadLength);
            }
        }

    }
}
