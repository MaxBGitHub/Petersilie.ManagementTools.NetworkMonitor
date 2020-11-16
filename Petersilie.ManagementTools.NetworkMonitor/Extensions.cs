
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

        /// <summary>
        /// Combines a high nibble and a low nibble to one single byte.
        /// A nibble is half a byte e.g. 4-bit.
        /// <para>Example for 0100 0101 (69):</para>
        /// <para>High nibble = 0100 (4)</para>
        /// <para>Low nibble = 0101 (5)</para>
        /// </summary>
        /// <param name="highNibble">High nibble.</param>
        /// <param name="lowNibble">Low nibble</param>
        /// <returns>Returns a byte consisting of both nibbles.</returns>
        public static byte AddLowNibble(this byte highNibble, byte lowNibble) {
            return (byte)((highNibble & 0xf) << 4 | lowNibble);
        }


        /// <summary>
        /// Combines a high nibble and a low nibble to one single byte.
        /// A nibble is half a byte e.g. 4-bit.
        /// <para>Example for 0100 0101 (69):</para>
        /// <para>High nibble = 0100 (4)</para>
        /// <para>Low nibble = 0101 (5)</para>
        /// </summary>        
        /// <param name="lowNibble">Low nibble</param>
        /// <param name="highNibble">High nibble.</param>
        /// <returns>Returns a byte consisting of both nibbles.</returns>
        public static byte AddHighNibble(this byte lowNibble, byte highNibble) {
            return (byte)((highNibble & 0xf) << 4 | lowNibble);
        }


        /// <summary>
        /// Sets the bit at the specified position to 0 or 1.
        /// </summary>
        /// <param name="b">The byte which bit gets set.</param>
        /// <param name="bitValue">Boolean representing a bit value.</param>
        /// <param name="pos">Zero based index of bit.</param>
        /// <returns>Returns the modified byte.</returns>
        public static byte SetBit(this byte b, bool bitValue, int index)
        {
            if (bitValue) {
                return b |= (byte)(1 << index);
            } /* Set bit to 1. */
            else {
                return (byte)(b & ~(1 << index));
            } /* Set bit to 0. */ 
            
        }


        /// <summary>
        /// Sets all bits from the specified starting position to the 
        /// specified inclusion position to 1 or 0.
        /// </summary>
        /// <param name="b">The byte which bits get to set.</param>
        /// <param name="bitValue">Boolean representing a bit value.</param>
        /// <param name="startIndex">Zero based start index (from inclusive).</param>
        /// <param name="endIndex">Zero based end index (to inclusive).</param>
        /// <returns>Returns the modified byte.</returns>
        public static byte SetBits(this byte b, bool bitValue, int startIndex, int endIndex)
        {
            byte r = b;
            while (startIndex <= endIndex) {
                r = r.SetBit(bitValue, startIndex);
                startIndex++;
            } /* Set all bits in range. */
            return r;
        }
        

        /// <summary>
        /// Gets the bit at the specified index.
        /// </summary>
        /// <param name="b">The byte to get the bit from.</param>
        /// <param name="index">Zero based index of bit to retreive.</param>
        /// <returns>Returns a single bit from a byte.</returns>
        public static byte GetBit(this byte b, int index) {
            return (byte)((b & (1 << index)) == 0 ? 0 : 1);
        }


        /// <summary>
        /// Gets each bit of the byte and stores in a byte array.
        /// </summary>
        /// <param name="b">The byte to get the bits from.</param>
        /// <returns>Returns an array containing the bits of the byte</returns>
        public static byte[] GetBits(this byte b)
        {
            byte[] bits = new byte[8];
            for (int i=7; i>=0; i--) {
                bits[i] = b.GetBit(i);
            }
            return bits;
        }
    }
}
