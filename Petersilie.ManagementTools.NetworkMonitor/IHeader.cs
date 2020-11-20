
namespace Petersilie.ManagementTools.NetworkMonitor
{
    public interface IHeader
    {
        /// <summary>
        /// Raw packet data.
        /// </summary>
        byte[] Packet { get; }

        /// <summary>
        /// Actual protocol.
        /// </summary>
        Protocol Protocol { get; }

        /// <summary>
        /// Converts the protocol back to a raw byte stream.
        /// </summary>
        /// <returns></returns>
        byte[] ToByte();

        /// <summary>
        /// Converts the protocol back to a raw stream.
        /// </summary>
        /// <returns></returns>
        System.IO.Stream ToStream();
    }
}
