using System;
using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /*  
    **   0           4           8            12         16           20          24           31
    **   |-------------------------------------------------------------------------------------|
    **   |          Type         |          Code         |              Checksum               |
    **   |-------------------------------------------------------------------------------------|
    **   |                                                                                     |
    **   |                                         Data                                        |
    **   |                                      (optional)                                     |
    **   |                                                                                     |
    **   |-------------------------------------------------------------------------------------|
    */
    public class ICMPHeader : IHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        public byte[] Packet { get; }
        /// <summary>
        /// <see cref="Protocol.ICMP"/>
        /// </summary>
        public Protocol Protocol { get; } = Protocol.ICMP;
        /// <summary>
        /// Contains the description for the 
        /// specific combination of type and code.
        /// </summary>
        public ICMPTypeCodeEntry TypeCodeDescription { get; }
        /// <summary>
        /// Bit 0-7. ICMP type, see <see cref="ICMPTypename"/>.
        /// </summary>
        public byte Type { get; }
        /// <summary>
        /// Bit 8-15. ICMP subtype.
        /// </summary>
        public byte Code { get; }
        /// <summary>
        /// Bit 16-31. Internet checksum (RFC 1071) for error checking.
        /// </summary>
        public ushort Checksum { get; }
        /// <summary>
        /// Bit 32-?.
        /// ICMP error messages contain a data section that includes a 
        /// copy of the entire IPv4 header, plus at leas the first eight
        /// bytes of data from the IPv4 packet that caused the error message.
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
                writer.Write(Type);
                writer.Write(Code);
                writer.Write(Checksum);
                writer.Write(Data);

                return mem;
            }
        }


        public byte[] ToByte()
        {
            if (null != Packet) {
                if (0 <= Packet.Length) {
                    // Return packet instead of parsing data.
                    return Packet;
                } /* Packet has data. */         
            } /* Packet is not null. */

            MemoryStream stream = ToStream() as MemoryStream;
            return stream.ToArray();
        }


        private ICMPTypeCodeMap _tcMap = new ICMPTypeCodeMap();

        /// <summary>
        /// Initializes a new instance of a ICMPHeader object.
        /// </summary>
        /// <param name="packet">Array containing ICMP 
        /// data that can be parsed and reconstructed.</param>
        public ICMPHeader(byte[] packet)
        {
            Packet = packet;

            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                Type = reader.ReadByte();
                Code = reader.ReadByte();
                Checksum = reader.ReadUInt16();
                TypeCodeDescription = _tcMap[Type, Code];

                int dataLength = (int)(packet.Length - mem.Position);
                Data = reader.ReadBytes(dataLength);
            }
        }
    }
}
