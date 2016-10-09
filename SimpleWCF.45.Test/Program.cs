using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleWCF;
using SimpleWCF.Discovery;
using System.ServiceModel;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace SimpleWCF._45.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEvent mre = new ManualResetEvent(false);

            DiscoveryService discoService = new DiscoveryService(2030);
            discoService.Start();

            var daemon = new WCFDaemon()
                .EnableAutoDiscovery(Dns.GetHostName(), 2030)
                .AddService<IPingService, PingService>(config =>
                     config.AddHttpBinding(9350)
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetDiscoveryProxy(Dns.GetHostName(), 2030, BindingType.WCF_Http);
            
            var sw = new Stopwatch();
            sw.Start();

            var c = 0;

            for (int i = 0; i < 1000000; i++)
            {
                var result = service.Ping();

                c++;

                if(sw.ElapsedMilliseconds >= 1000)
                {
                    Console.WriteLine("{0:n0} messages per second", c);

                    c = 0;
                    sw.Restart();
                }
            }

            daemon.Stop();

            mre.WaitOne();
        }
    }
}
