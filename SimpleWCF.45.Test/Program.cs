using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleWCF;
using System.ServiceModel;
using System.Net;

namespace SimpleWCF._45.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var daemon = new WCFDaemon()
                .AddService<IPingService,PingService>(config => 
                    config.AddHttpBinding(9060)
                );

            daemon.Start();
            
            IPingService service = new WCFProxy<IPingService>().GetHttpProxy(Dns.GetHostName(), 9060);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine("{0}", service.Ping());
            }

            Console.ReadKey();

            daemon.Stop();
        }
    }
}
