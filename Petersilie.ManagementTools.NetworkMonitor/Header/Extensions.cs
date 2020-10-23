using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor.Header
{
    public static class Extensions
    {
        public static byte LowNibble(this byte b) {
            return (byte)(b & 0x0f);
        }

        public static byte HighNibble(this byte b) {
            return (byte)((b >> 4) & 0x0f);
        }
    }
}
