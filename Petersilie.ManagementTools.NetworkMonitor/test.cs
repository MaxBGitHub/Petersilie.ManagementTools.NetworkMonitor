using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    class test
    {
        static void PacketReceived(object sender, PacketEventArgs e)
        {
            // Something...
            var header = new IPv4Header(e.Packet);            
            if (header.Protocol == Protocol.ICMP)
            {
                var icmp = new ICMPHeader(header.Data);
                Console.WriteLine(header.SourceAddress + " - " + header.DestinationAddress + " - " + icmp.TypeCodeDescription.Description);
            }
        }


        private static void ErrorReceived(object sender, MonitorExceptionEventArgs e)
        {
            Console.WriteLine(e.Error.Message);
        }


        private static void StartMon(object ip)
        {
            string sip = ip.ToString();
            var netMons = NetworkMonitor.BindInterfaces();
            //var netMon = new NetworkMonitor(System.Net.IPAddress.Parse(sip), 0);
            //netMon.PacketReceived += PacketReceived;
            foreach (var mon in netMons)
            {
                mon.PacketReceived += PacketReceived;
                mon.OnError += ErrorReceived;
                mon.Begin();
            }
            
        }


        static void Main(string[] args)
        {
            string ip1 = "127.0.0.1";
            string ip2 = "10.1.0.200";

            new Thread(new ParameterizedThreadStart(StartMon)).Start(ip1);
            new Thread(new ParameterizedThreadStart(StartMon)).Start(ip2);

            while (true) { }

        }

    }
}
