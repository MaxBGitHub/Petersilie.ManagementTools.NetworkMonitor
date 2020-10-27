
namespace Petersilie.ManagementTools.NetworkMonitor
{
    public interface IIPHeader
    {
        IPVersion IPVersion { get; }
        System.Net.IPAddress SourceAddress { get; }
        System.Net.IPAddress DestinationAddress { get; }
    }
}
