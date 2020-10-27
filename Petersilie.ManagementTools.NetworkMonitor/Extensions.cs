
namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Extension class for <see cref="byte"/>
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the low part nibble of a byte.
        /// A nibble is half a byte e.g. 4-bit.
        /// </summary>
        /// <param name="b">Byte to get nibble from.</param>
        /// <returns></returns>
        public static byte LowNibble(this byte b) {
            return (byte)(b & 0x0f);
        }

        /// <summary>
        /// Gets the high part nibble of a byte.
        /// A nibble is half a byte e.g. 4-bit.
        /// </summary>
        /// <param name="b">Byte to get nibble from.</param>
        /// <returns></returns>
        public static byte HighNibble(this byte b) {
            return (byte)((b >> 4) & 0x0f);
        }
    }
}
