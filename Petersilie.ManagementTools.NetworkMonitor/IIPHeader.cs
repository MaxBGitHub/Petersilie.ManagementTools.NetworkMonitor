
namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Interface for IPv4 and IPv6 headers.
    /// </summary>
    public interface IIPHeader : IHeader
    {
        /// <summary>
        /// IP-Version of header.
        /// </summary>
        IPVersion IPVersion { get; }
        /// <summary>
        /// Source address.
        /// </summary>
        System.Net.IPAddress SourceAddress { get; }
        /// <summary>
        /// Destination address.
        /// </summary>
        System.Net.IPAddress DestinationAddress { get; } 
    }
}
