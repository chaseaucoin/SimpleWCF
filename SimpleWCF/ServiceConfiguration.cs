using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace SimpleWCF
{
    public class ServiceConfiguration<TServiceContract, TServiceImplementation>
    {
        ServiceHost _host;

        public ServiceConfiguration()
        {
            _host = new ServiceHost(typeof(TServiceImplementation));
            var behavior = _host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behavior.ConcurrencyMode = ConcurrencyMode.Multiple;
            behavior.MaxItemsInObjectGraph = int.MaxValue;
            behavior.InstanceContextMode = InstanceContextMode.PerCall;

            var throttle = new ServiceThrottlingBehavior();
                
            throttle.MaxConcurrentCalls = 1000;
            throttle.MaxConcurrentInstances = 1000;
            throttle.MaxConcurrentSessions = 1000;

            _host.Description.Behaviors.Add(throttle);
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> AddHttpBinding(int port)
        {
            var uriBuilder = new UriBuilder("http", "0", port, typeof(TServiceContract).Name);

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            _host.AddServiceEndpoint(typeof(TServiceContract), binding, uriBuilder.ToString());

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> SingleInstance()
        {
            var behavior = _host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behavior.InstanceContextMode = InstanceContextMode.Single;

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> InstancePerCall()
        {
            var behavior = _host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behavior.InstanceContextMode = InstanceContextMode.PerCall;

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> InstancePerSession()
        {
            var behavior = _host.Description.Behaviors.Find<ServiceBehaviorAttribute>();
            behavior.InstanceContextMode = InstanceContextMode.PerSession;

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> TweakHost(Action<ServiceHost> hostConfigurationAction)
        {
            hostConfigurationAction(_host);

            return this;
        }

        internal ServiceHost CreateServiceHost()
        {
            return _host;
        }
    }
}
