using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petersilie.ManagementTools.NetworkMonitor
{
    class test
    {
        private static void OnError(object sender, PacketErrorEventArgs e)
        {
            Console.WriteLine(e.Exception.Message);
        }


        static void Main(string[] args)
        {
            var ip = System.Net.IPAddress.Parse("10.1.0.200");
            var netMon = new NetworkMonitor(ip);
            netMon.OnError += OnError;
            netMon.Begin();
            while (true) { }
        }

    }
}
