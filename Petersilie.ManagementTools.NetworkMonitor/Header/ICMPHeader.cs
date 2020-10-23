using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace Petersilie.ManagementTools.NetworkMonitor.Header
{
    public class ICMPHeader
    {
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

        public byte Type { get; }

        public byte Code { get; }

        public ushort Checksum { get; }

        public byte[] Data { get; }


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
