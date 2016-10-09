using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace SimpleWCF
{
    public class ServiceConfiguration<TServiceContract, TServiceImplementation>
    {
        ServiceHost _host;
        List<ServiceEndpoint> _serviceEndpoints;

        /// <summary>
        /// Gets or sets the endpoint details.
        /// </summary>
        /// <value>
        /// The endpoint details.
        /// </value>
        public List<EndpointDetails> EndpointDetails { get; set; }

        public ServiceConfiguration()
        {
            _host = new ServiceHost(typeof(TServiceImplementation));
            _serviceEndpoints = new List<ServiceEndpoint>();
            EndpointDetails = new List<EndpointDetails>();

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
        
        private void AddServiceEndpoint(Binding binding, string uri, BindingType bindingType, string securityMode = "None", string thumbPrint = null)
        {
            var contractDecription = ContractDescription.GetContract(typeof(TServiceContract));
            var endpoint = new ServiceEndpoint(contractDecription, binding, new EndpointAddress(uri));
            var endpointDetails = new EndpointDetails()
            {
                Address = endpoint.Address.Uri,
                Name = endpoint.Name,
                ContractShortName = typeof(TServiceContract).Name,
                ContractFullName = typeof(TServiceContract).FullName,
                Binding = bindingType.ToString(),
                SecurityMode = securityMode,
                ThumbPrint = thumbPrint
            };

            this.EndpointDetails.Add(endpointDetails);
            _serviceEndpoints.Add(endpoint);
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> AddHttpBinding(int port)
        {
            var uriBuilder = new UriBuilder("http", "0", port, typeof(TServiceContract).Name);

            var binding = new BasicHttpBinding(BasicHttpSecurityMode.None);

            AddServiceEndpoint(binding, uriBuilder.ToString(), BindingType.WCF_Http, "None", null);

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> AddSecureHttpBinding(int port, string subject)
        {
            var uriBuilder = new UriBuilder("http", "0", port, typeof(TServiceContract).Name);

            var binding = new WSHttpBinding(SecurityMode.Message);
            binding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

            _host.Credentials.ServiceCertificate
                .SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, subject);

            AddServiceEndpoint(binding, uriBuilder.ToString(), BindingType.WCF_SecureHttp, "Message", _host.Credentials.ServiceCertificate.Certificate.Thumbprint);

            return this;
        }

        public ServiceConfiguration<TServiceContract, TServiceImplementation> EnableDebug()
        {
            var behavior = _host.Description.Behaviors.Find<ServiceDebugBehavior>();

            if (behavior == null)
                _host.Description.Behaviors.Add(
                    new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });
            else
                behavior.IncludeExceptionDetailInFaults = true;

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
            foreach (var endpoint in _serviceEndpoints)
            {
                _host.AddServiceEndpoint(endpoint);
            }

            return _host;
        }
    }
}
