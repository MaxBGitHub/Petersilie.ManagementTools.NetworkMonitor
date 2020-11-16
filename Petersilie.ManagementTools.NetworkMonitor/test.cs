using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    class test
    {
        static void PacketReceived(object sender, PacketEventArgs e)
        {
            var header = new IPv4Header(e.Packet);
            var bytes = header.GetBytes();

            Console.WriteLine(string.Join(" ", e.Packet));
            Console.WriteLine(string.Join(" ", bytes));
        }

        static void Main(string[] args)
        {
            var netMon = new NetworkMonitor(System.Net.IPAddress.Parse("127.0.0.1"), 0);
            netMon.PacketReceived += PacketReceived;
            netMon.Begin();

            while (true) { }

        }

    }
}
