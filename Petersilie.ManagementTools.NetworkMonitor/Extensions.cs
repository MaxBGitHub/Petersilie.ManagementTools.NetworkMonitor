
namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Extension class for <see cref="byte"/>
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Gets the low part nibble of a byte.
        /// A nibble is half a byte e.g. 4-Bit.
        /// </summary>
        /// <param name="b">Byte to get nibble from.</param>
        /// <returns></returns>
        public static byte LowNibble(this byte b) {
            return (byte)(b & 0x0f);
        }

        public static byte LowNibble(this int i) {
            return (byte)(i & 0x20);
        }

        public static byte HighNibble(this int i) {
            return (byte)((i >> 4) & 0x20);
        }

        /// <summary>
        /// Gets the high part nibble of a byte.
        /// A nibble is half a byte e.g. 4-Bit.
        /// </summary>
        /// <param name="b">Byte to get nibble from.</param>
        /// <returns></returns>
        public static byte HighNibble(this byte b) {
            return (byte)((b >> 4) & 0x0f);
        }

        /// <summary>
        /// Combines a high nibble and a low nibble to one single byte.
        /// A nibble is half a byte e.g. 4-Bit.
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
        /// A nibble is half a byte e.g. 4-Bit.
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
        /// Sets the Bit at the specified position to 0 or 1.
        /// </summary>
        /// <param name="b">The byte which Bit gets set.</param>
        /// <param name="bitValue">Boolean representing a Bit value.</param>
        /// <param name="pos">Zero based index of Bit.</param>
        /// <returns>Returns the modified byte.</returns>
        public static byte SetBit(this byte b, bool bitValue, int index)
        {
            if (bitValue) {
                return b |= (byte)(1 << index);
            } /* Set Bit to 1. */
            else {
                return (byte)(b & ~(1 << index));
            } /* Set Bit to 0. */ 
            
        }


        /// <summary>
        /// Sets all bits from the specified starting position to the 
        /// specified inclusion position to 1 or 0.
        /// </summary>
        /// <param name="b">The byte which bits get to set.</param>
        /// <param name="bitValue">Boolean representing a Bit value.</param>
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
        /// Gets the Bit at the specified index.
        /// </summary>
        /// <param name="b">The byte to get the Bit from.</param>
        /// <param name="index">Zero based index of Bit to retreive.</param>
        /// <returns>Returns a single Bit from a byte.</returns>
        public static bool GetBit(this byte b, int index) {
            return (b & (1 << index)) != 0;
        }


        /// <summary>
        /// Gets each Bit of the byte and stores in a byte array.
        /// </summary>
        /// <param name="b">The byte to get the bits from.</param>
        /// <returns>Returns an array containing the bits of the byte</returns>
        public static bool[] GetBits(this byte b)
        {
            bool[] bits = new bool[8];
            for (int i=0; i<8; i++) {
                bits[i] = (b & (1 << i)) != 0;
            }
            return bits;
        }


        /// <summary>
        /// Gets each Bit of the byte within the specified range.
        /// </summary>
        /// <param name="b">The byte to get the bits from.</param>
        /// <param name="from">Zero based start index.</param>
        /// <param name="to">Zero based end index.</param>
        /// <returns></returns>
        public static bool[] GetBits(this byte b, int from, int to)
        {
            int n = -1;
            bool[] bits = new bool[to - from];
            for (int i=from; i<to; i++) {
                bits[++n] = (b & (1 << i)) != 0;
            }
            return bits;
        }


        public static bool[] GetBits(this int i, int from, int to)
        {
            int n = -1;
            bool[] bits = new bool[to - from];
            for (int x=from; x<to; x++) {
                bits[++n] = (i & (1 << x)) != 0;
            }
            return bits;
        }
        

        /// <summary>
        /// Creates a new array that contains the new bits aswell.
        /// </summary>
        /// <param name="src">Array to which to add the bits.</param>
        /// <param name="b">The byte to get the bits from</param>
        /// <returns></returns>
        public static bool[] AddBits(this bool[] src, byte b)
        {
            var bits = b.GetBits();
            bool[] arr = new bool[src.Length + bits.Length];
            src.CopyTo(arr, 0);
            bits.CopyTo(arr, src.Length);

            return arr;
        }


        /// <summary>
        /// Creates a new array that contains the new bits aswell.
        /// </summary>
        /// <param name="src">Array to which to add the bits.</param>
        /// <param name="bits">An array of bits which are added.</param>
        /// <returns></returns>
        public static bool[] AddBits(this bool[] src, bool[] bits)
        {
            bool[] arr = new bool[src.Length + bits.Length];
            src.CopyTo(arr, 0);
            bits.CopyTo(arr, src.Length);

            return arr;
        }


        /// <summary>
        /// Converts an array of bits to an integer.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static int ToInt(this bool[] bits)
        {
            string sBinary = string.Empty;
            for (int i = bits.Length-1; i >= 0; i--) {
                sBinary += bits[i] ? "1" : "0";
            }
            return System.Convert.ToInt32(sBinary, 2);
        }


        /// <summary>
        /// Converts an array of bits to a byte.
        /// </summary>
        /// <param name="bits"></param>
        /// <returns></returns>
        public static byte ToByte(this bool[] bits)
        {
            if (bits.Length > 8) {
                throw new System.ArgumentOutOfRangeException(nameof(bits), 
                    "Bit array exceeded the maximum amount of 8-bits.");
            }

            string sBinary = string.Empty;
            for (int i=bits.Length-1; i>=0; i--) {
                sBinary += bits[i] ? "1" : "0";
            }
            return System.Convert.ToByte(sBinary, 2);
        }
    }
}
