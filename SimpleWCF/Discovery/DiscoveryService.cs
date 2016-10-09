using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using Microsoft.AspNet.SignalR;
using System.Timers;

namespace SimpleWCF.Discovery
{
    public class DiscoveryService : IDisposable
    {
        private class Startup
        {
            // This code configures Web API. The Startup class is specified as a type
            // parameter in the WebApp.Start method.
            public void Configuration(IAppBuilder appBuilder)
            {
                appBuilder.MapSignalR();
            }
        }

        string _baseAddress;
        IDisposable app;
        IHubContext _disoveryHub;
        Timer _timer;


        public DiscoveryService(int port)
        {
            _baseAddress = string.Format("http://+:{0}/",port);
        }

        public void Start()
        {
            app = WebApp.Start<Startup>(url: _baseAddress);
            _disoveryHub = GlobalHost.ConnectionManager.GetHubContext<DiscoveryHub>();

            _timer = new Timer(1000) { AutoReset = true };
            _timer.Elapsed += (sender, eventArgs) =>
            {
                _disoveryHub
                    .Clients
                    .Group("Services")
                    .CheckHealth();
            };

            _timer.Start();
        }

        public void Dispose()
        {
            app.Dispose();
            _timer.Dispose();
        }
    }
}
