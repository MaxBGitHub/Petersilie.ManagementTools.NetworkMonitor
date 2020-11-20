using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    /// <summary>
    /// Contains the type, code and specific description
    /// of an ICMP datagram.
    /// </summary>
    public struct ICMPTypeCodeEntry
    {
        public ICMPTypename Typename
        {
            get
            {
                ICMPTypename tn;
                if (Enum.TryParse(Type.ToString(), out tn))
                {
                    if (Enum.IsDefined(typeof(ICMPTypename), tn))
                    {
                        return tn;
                    }
                }
                return ICMPTypename.UNDEFINED;
            }
        }

        public int Type;
        public int Code;
        public string Description;

        public ICMPTypeCodeEntry(int t, int c, string d)
        {
            Type = t;
            Code = c;
            Description = d;
        }
    }
}
