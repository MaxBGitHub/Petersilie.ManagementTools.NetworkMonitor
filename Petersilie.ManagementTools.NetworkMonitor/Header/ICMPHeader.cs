using System;
using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor.Header
{
    public class ICMPHeader
    {
        /// <summary>
        /// Translated <see cref="Type"/> property.
        /// </summary>
        public ICMPTypename Typename
        {
            get
            {
                ICMPTypename t;
                if (Enum.TryParse(Type.ToString(), out t)) {
                    if (Enum.IsDefined(typeof(ICMPTypename), Type)) {
                        return t;
                    }
                }
                return ICMPTypename.UNDEFINED;
            }
        }

        /// <summary>
        /// ICMP type, see <see cref="ICMPTypename"/>.
        /// </summary>
        public byte Type { get; }

        /// <summary>
        /// ICMP subtype.
        /// </summary>
        public byte Code { get; }

        /// <summary>
        /// Internet checksum (RFC 1071) for error checking.
        /// </summary>
        public ushort Checksum { get; }

        /// <summary>
        /// ICMP error messages contain a data section that includes a 
        /// copy of the entire IPv4 header, plus at leas the first eight
        /// bytes of data from the IPv4 packet that caused the error message.
        /// </summary>
        public byte[] Data { get; }


        /// <summary>
        /// Initializes a new instance of a ICMPHeader object.
        /// </summary>
        /// <param name="packet">Array containing ICMP 
        /// data that can be parsed and reconstructed.</param>
        public ICMPHeader(byte[] packet)
        {
            using (var mem = new MemoryStream(packet))
            using (var reader = new BinaryReader(mem))
            {
                Type = reader.ReadByte();
                Code = reader.ReadByte();
                Checksum = reader.ReadUInt16();

                int dataLength = (int)(packet.Length - mem.Position);
                Data = reader.ReadBytes(dataLength);
            }
        }
    }
}
