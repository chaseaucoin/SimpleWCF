using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceModel;

namespace SimpleWCF
{
    public class WCFDaemon
    {
        private List<ServiceHost> _hosts = new List<ServiceHost>();

        private List<Action> _AddBindingList = new List<Action>();

        private Action _AddDiscovery = () => {};
        
        public WCFDaemon AddService<TServiceContract, TServiceImplementation>(Action<ServiceConfiguration<TServiceContract, TServiceImplementation>> buildAction)
        {
            var serviceBuilder = new ServiceConfiguration<TServiceContract, TServiceImplementation>();

            buildAction(serviceBuilder);

            var host = serviceBuilder.CreateServiceHost();

            _hosts.Add(host);

            return this;
        }

        public void Start()
        {
            foreach(var host in _hosts)
            {
                host.Open();
            }
        }

        public void Stop()
        {
            foreach (var host in _hosts)
            {
                host.Close();
            }
        }
    }
}
