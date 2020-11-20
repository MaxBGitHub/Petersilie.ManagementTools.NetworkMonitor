using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Utility class for identifying the Internet Protocol Version
    /// of an IP packet and to parse a packet.
    /// </summary>
    public static class IPHeaderUtil
    {
        /// <summary>
        /// Gets the specific Internet Protocol Version from the 
        /// raw data stream.
        /// </summary>
        /// <param name="packet">Raw packet data.</param>
        /// <returns></returns>
        public static IPVersion GetVersion(byte[] packet)
        {
            if (null == packet) {
                return IPVersion.Other;
            }

            if (0 == packet.Length) {
                return IPVersion.Other;
            }

            using (var mem = new MemoryStream(packet, 0, 1))
            using (var reader = new BinaryReader(mem))
            {
                byte b = reader.ReadByte();
                byte Version = b.HighNibble();
                if (4 == Version) {
                    return IPVersion.IPv4;
                } else if (6 == Version) {
                    return IPVersion.IPv6;
                } else {
                    return IPVersion.Other;
                }
            }
        }


        /// <summary>
        /// Parses a raw packet into an IPv4 or IPv6 header.
        /// If it is not an Internet Protocol packet than the 
        /// function returns null.
        /// </summary>
        /// <param name="packet">Raw IP packet.</param>
        /// <returns></returns>
        public static IIPHeader Parse(byte[] packet)
        {
            if (null == packet) {
                return null;
            }

            if (0 == packet.Length) {
                return null;
            }

            using (var mem = new MemoryStream(packet, 0, 1))
            using (var reader = new BinaryReader(mem))
            {
                byte b = reader.ReadByte();
                byte Version = b.HighNibble();
                if (Version == 4) {
                    return new IPv4Header(packet);
                } else if (Version == 6) {
                    return new IPv6Header(packet);
                } else {
                    return null;
                }
            }
        }
    }
}
