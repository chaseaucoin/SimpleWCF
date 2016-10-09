using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using SimpleWCF.Discovery;

namespace SimpleWCF.Unit.Test
{
    [TestClass]
    public class SimpleWCFtests
    {


        [TestMethod]
        public void HttpBindingTest()
        {
            var daemon = new WCFDaemon()
                .AddService<IPingService, PingService>(config =>
                     config.AddHttpBinding(2027)
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetHttpProxy(Dns.GetHostName(), 2027);

            var result = service.Ping();

            daemon.Stop();

            Assert.AreEqual(result, "pong");
        }

        [TestMethod]
        public void DebugTest()
        {
            var daemon = new WCFDaemon()
                .AddService<IPingService, PingService>(config =>
                     config
                        .AddHttpBinding(2028)
                        .EnableDebug()
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetHttpProxy(Dns.GetHostName(), 2028);

            try
            {
                var result = service.ThrowAnException();
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.InnerException.Message, "I didn't work");
            }
            finally
            {
                daemon.Stop();
            }

        }

        [TestMethod]
        public void SecureHttpBindingTest()
        {
            var daemon = new WCFDaemon()
                .AddService<IPingService, PingService>(config =>
                     config.AddSecureHttpBinding(2029, "NVIDIA GameStream Server")
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetSecureHttpProxy("localhost", 2029, "NVIDIA GameStream Server");

            var result = service.Ping();

            daemon.Stop();

            Assert.AreEqual(result, "pong");
        }

        [TestMethod]
        public void Discovery_HttpBindingTest()
        {
            DiscoveryService discoService = new DiscoveryService(2030);
            discoService.Start();

            var daemon = new WCFDaemon()
                .EnableAutoDiscovery(Dns.GetHostName(), 2030)
                .AddService<IPingService, PingService>(config =>
                     config.AddHttpBinding(9350)
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetDiscoveryProxy(Dns.GetHostName(), 2030, BindingType.WCF_Http);

            string result = service.Ping();

            daemon.Stop();

            Assert.AreEqual(result, "pong");

        }


        [TestMethod]
        public void Discovery_Secure_HttpBindingTest()
        {
            DiscoveryService discoService = new DiscoveryService(2031);
            discoService.Start();

            var daemon = new WCFDaemon()
                .EnableAutoDiscovery(Dns.GetHostName(), 2031)
                .AddService<IPingService, PingService>(config =>
                     config.AddSecureHttpBinding(9351, "NVIDIA GameStream Server")
                );

            daemon.Start();

            IPingService service = new WCFProxy<IPingService>().GetDiscoveryProxy(Dns.GetHostName(), 2031, BindingType.WCF_SecureHttp);

            string result = service.Ping();

            daemon.Stop();

            Assert.AreEqual(result, "pong");
        }
    }
}
