using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    public interface IIPHeader
    {
        IPVersion IPVersion { get; }
        System.Net.IPAddress SourceAddress { get; }
        System.Net.IPAddress DestinationAddress { get; }
    }
}
