using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;
using SimpleWCF.Discovery;

namespace SimpleWCF
{
    public class WCFDaemon : IDisposable
    {
        private List<ServiceHost> _hosts;
        private List<EndpointDetails> _endpointDetails;
        private DiscoveryClient _discoveryClient;

        public WCFDaemon()
        {
            System.Net.WebRequest.DefaultWebProxy = null;
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            System.Net.ServicePointManager.UseNagleAlgorithm = false;

            _hosts = new List<ServiceHost>();
            _endpointDetails = new List<EndpointDetails>();
        }

        

        public WCFDaemon EnableAutoDiscovery(string discoveryHost, int port)
        {
            if (_discoveryClient != null)
                return this;

            _discoveryClient = new DiscoveryClient(port);            

            return this;
        }

        public WCFDaemon AddService<TServiceContract, TServiceImplementation>(Action<ServiceConfiguration<TServiceContract, TServiceImplementation>> buildAction)
        {
            var serviceBuilder = new ServiceConfiguration<TServiceContract, TServiceImplementation>();

            buildAction(serviceBuilder);

            var host = serviceBuilder.CreateServiceHost();
            
            _hosts.Add(host);
            _endpointDetails.AddRange(serviceBuilder.EndpointDetails);

            return this;
        }

        public void Start()
        {
            foreach(var host in _hosts)
            {
                host.Open();                
            }

            if (_discoveryClient != null)
            {
                foreach (var serviceEndpoint in _endpointDetails)
                {
                    _discoveryClient.AddService(serviceEndpoint);
                }
            }
        }

        public void Stop()
        {
            foreach (var host in _hosts)
            {
                host.Close();
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
