using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    internal class ICMPTypeCodeMap : HashSet<ICMPTypeCodeEntry>
    {
        public ICMPTypeCodeEntry this[int type, int code]
        {
            get {
                return this.Where(entry => entry.Type == type && entry.Code == code)
                    .First();
            }
        }


        public ICMPTypeCodeEntry[] this[int type]
        {
            get {
                return this.Where(entry => entry.Type == type).ToArray();
            }
        }


        private void InitTypeCodeMap()
        {
            this.Add(new ICMPTypeCodeEntry(0, 0, "Echo reply"));

            this.Add(new ICMPTypeCodeEntry(1, 0, "Reserved"));
            this.Add(new ICMPTypeCodeEntry(2, 0, "Reserved"));

            this.Add(new ICMPTypeCodeEntry(3, 0, "Destination network unreachable"));
            this.Add(new ICMPTypeCodeEntry(3, 1, "Destination host unreachable"));
            this.Add(new ICMPTypeCodeEntry(3, 2, "Destination protocol unreachable"));
            this.Add(new ICMPTypeCodeEntry(3, 3, "Destination port unreachable"));
            this.Add(new ICMPTypeCodeEntry(3, 4, "Fragmentation required, and DF flag set"));
            this.Add(new ICMPTypeCodeEntry(3, 5, "Source route failed"));
            this.Add(new ICMPTypeCodeEntry(3, 6, "Destination network unkown"));
            this.Add(new ICMPTypeCodeEntry(3, 7, "Destination host unkown"));
            this.Add(new ICMPTypeCodeEntry(3, 8, "Source host isolated"));
            this.Add(new ICMPTypeCodeEntry(3, 8, "Network administratively prohibited"));
            this.Add(new ICMPTypeCodeEntry(3, 10, "Host administratively prohibited"));
            this.Add(new ICMPTypeCodeEntry(3, 11, "Network unreachable for ToS"));
            this.Add(new ICMPTypeCodeEntry(3, 12, "Host unreachable for ToS"));
            this.Add(new ICMPTypeCodeEntry(3, 13, "Communication administratively prohibited"));
            this.Add(new ICMPTypeCodeEntry(3, 14, "Host Precedence Violation"));
            this.Add(new ICMPTypeCodeEntry(3, 15, "Precedence cutoff in effect"));

            this.Add(new ICMPTypeCodeEntry(4, 0, "Depreciated"));

            this.Add(new ICMPTypeCodeEntry(5, 0, "Redirect Datagram for Network"));
            this.Add(new ICMPTypeCodeEntry(5, 1, "Redirect Datagram for Host"));
            this.Add(new ICMPTypeCodeEntry(5, 2, "Redirect Datagram for the ToS & network"));
            this.Add(new ICMPTypeCodeEntry(5, 3, "Redirect Datagram for the ToS & host"));

            this.Add(new ICMPTypeCodeEntry(6, 0, "Depreciated"));

            this.Add(new ICMPTypeCodeEntry(7, 0, "Unassigned"));

            this.Add(new ICMPTypeCodeEntry(8, 0, "Echo request"));

            this.Add(new ICMPTypeCodeEntry(9, 0, "Router Advertisment"));

            this.Add(new ICMPTypeCodeEntry(10, 0, "Router discovery/selection/solicitation"));

            this.Add(new ICMPTypeCodeEntry(11, 0, "TTL expired in transit"));
            this.Add(new ICMPTypeCodeEntry(11, 1, "Fragment reassembly time exceeded"));

            this.Add(new ICMPTypeCodeEntry(12, 0, "Pointer indicates the error"));
            this.Add(new ICMPTypeCodeEntry(12, 1, "Missing a required option"));
            this.Add(new ICMPTypeCodeEntry(12, 2, "Bad length"));

            this.Add(new ICMPTypeCodeEntry(13, 0, "Timestamp"));

            this.Add(new ICMPTypeCodeEntry(14, 0, "Timestamp reply"));

            this.Add(new ICMPTypeCodeEntry(15, 0, "Information Request"));

            this.Add(new ICMPTypeCodeEntry(16, 0, "Information Reply"));

            this.Add(new ICMPTypeCodeEntry(17, 0, "Address Mask Request"));

            this.Add(new ICMPTypeCodeEntry(18, 0, "Address Mask Reply"));

            this.Add(new ICMPTypeCodeEntry(19, 0, "Reserved for security"));

            this.Add(new ICMPTypeCodeEntry(20, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(21, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(22, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(23, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(24, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(25, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(26, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(27, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(28, 0, "Reserved for robustness experiment"));
            this.Add(new ICMPTypeCodeEntry(29, 0, "Reserved for robustness experiment"));

            this.Add(new ICMPTypeCodeEntry(30, 0, "Information Request"));

            this.Add(new ICMPTypeCodeEntry(31, 0, "Datagram Conversion Error"));

            this.Add(new ICMPTypeCodeEntry(32, 0, "Mobile Host Redirect"));

            this.Add(new ICMPTypeCodeEntry(33, 0, "Where-Are-You (originally meant for IPv6)"));

            this.Add(new ICMPTypeCodeEntry(34, 0, "Here-I-Am (originally meant for IPv6"));

            this.Add(new ICMPTypeCodeEntry(35, 0, "Mobile Registration Request"));

            this.Add(new ICMPTypeCodeEntry(36, 0, "Mobile Registration Reply"));

            this.Add(new ICMPTypeCodeEntry(37, 0, "Domain Name Request"));

            this.Add(new ICMPTypeCodeEntry(38, 0, "Domain Name Reply"));

            this.Add(new ICMPTypeCodeEntry(39, 0, "SKIP Algorithm Discovery Protocol, Simple Key-Management for Internet Protocol"));

            this.Add(new ICMPTypeCodeEntry(40, 0, "Photuris, Security failures"));

            this.Add(new ICMPTypeCodeEntry(41, 0, "ICMP for experimental mobility protocol such as Seamoby"));

            this.Add(new ICMPTypeCodeEntry(42, 0, "Request Extended Echo (XPing)"));

            this.Add(new ICMPTypeCodeEntry(43, 0, "No Errror"));
            this.Add(new ICMPTypeCodeEntry(43, 1, "Malformed Query"));
            this.Add(new ICMPTypeCodeEntry(43, 2, "No Such Interface"));
            this.Add(new ICMPTypeCodeEntry(43, 3, "No Such Table Entry"));
            this.Add(new ICMPTypeCodeEntry(43, 4, "Multiple Interfaces Satisfy Query"));

            for (int i=44; i<=252; i++) {
                this.Add(new ICMPTypeCodeEntry(i, 0, "Reserved"));
            }

            this.Add(new ICMPTypeCodeEntry(253, 0, "RFC3692-style Experiment 1 (RFC 4727)"));
            this.Add(new ICMPTypeCodeEntry(254, 0, "RFC3692-style Experiment 2 (RFC 4727)"));
            this.Add(new ICMPTypeCodeEntry(255, 0, "Reserved"));
        }



        public ICMPTypeCodeMap()
        {
            InitTypeCodeMap();
        }
    }
}
