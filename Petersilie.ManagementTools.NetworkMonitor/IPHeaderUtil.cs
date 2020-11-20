using System.IO;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public static class IPHeaderUtil
    {
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
