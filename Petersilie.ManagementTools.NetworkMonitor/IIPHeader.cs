
namespace Petersilie.ManagementTools.NetworkMonitor
{
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
