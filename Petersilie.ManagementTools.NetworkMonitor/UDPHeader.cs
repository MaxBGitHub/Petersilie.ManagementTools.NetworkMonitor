using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /*  
    **   0           4           8            12         16           20          24           31
    **   |-------------------------------------------------------------------------------------|
    **   |                  Source port                  |           Destination port          |
    **   |-------------------------------------------------------------------------------------|
    **   |                     Length                    |           Checksum                  |
    **   |-------------------------------------------------------------------------------------|
    **   |                                                                                     |
    **   |                                       Data                                          |
    **   |                                                                                     |
    **   |-------------------------------------------------------------------------------------| 
    */
    public class UDPHeader : IHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        public byte[] Packet { get; }
        /// <summary>
        /// <see cref="Protocol.UDP"/>
        /// </summary>
        public Protocol Protocol { get; } = Protocol.UDP;
        /// <summary>
        /// Bits 0-15. Contains the sender's port.
        /// </summary>
        public ushort SourcePort { get; }
        /// <summary>
        /// Bits 16-31. Contains the receiver's port.
        /// </summary>
        public ushort DestinationPort { get; }
        /// <summary>
        /// Specifies length in bytes of UDP header and data.
        /// Minimum length is 8 bytes, the length of the headers.
        /// </summary>
        public ushort Length { get; }
        /// <summary>
        /// Checksum field may be used for error-checking of the header and
        /// data. This field is optional in IPv4.
        /// </summary>
        public ushort Checksum { get; }
        /// <summary>
        /// The payload conaining any additional data.
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

            mem = new MemoryStream();
            using (var writer = new BinaryWriter(mem))
            {
                writer.Write(SourcePort);
                writer.Write(DestinationPort);
                writer.Write(Length);
                writer.Write(Checksum);
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


        public UDPHeader(byte[] packet)
        {
            Packet = packet;

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
