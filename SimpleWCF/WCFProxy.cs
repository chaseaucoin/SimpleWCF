using CupcakeFactory.SimpleProxy;
using SimpleWCF.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Threading.Tasks;

namespace SimpleWCF
{
    public class WCFProxy<TServiceContract>
    {
        static ChannelFactory<TServiceContract> _channelFactory;
        static SimpleProxy<TServiceContract> _proxy;
        static DiscoveryClient _discoveryClient;
        static List<EndpointAddress> _serviceEndpoint;
        static BindingType _discoveryBindingType;

        static WCFProxy()
        {
            _serviceEndpoint = new List<EndpointAddress>();
            _proxy = new SimpleProxy<TServiceContract>(ChannelProxy);
        }

        public TServiceContract GetDiscoveryProxy(string host, int port, BindingType bindingType)
        {
            _discoveryBindingType = bindingType;
            
            _discoveryClient = new DiscoveryClient(port);
            _discoveryClient.UpdateEndpoints += newEndpoints => {
                lock (_serviceEndpoint)
                {
                    var _subject = _channelFactory.Credentials.ClientCertificate.Certificate.Subject;

                    _serviceEndpoint = newEndpoints
                        .Where(x => x.Binding == bindingType.ToString())
                        .Select(x => new EndpointAddress(x.Address, EndpointIdentity.CreateDnsIdentity(_subject)))
                        .ToList();
                }
            };

            var endpoints = _discoveryClient
                .ListenForEndpoints<TServiceContract>()
                .Where(x => x.Binding == bindingType.ToString());
            
            if (bindingType == BindingType.WCF_Http)
                _channelFactory = CreateHttpFactory();

            if (bindingType == BindingType.WCF_SecureHttp)
                    _channelFactory = CreateSecureHttpFactoryFromThumbprint(endpoints.FirstOrDefault().ThumbPrint);

            var subject = _channelFactory
                .Credentials
                .ClientCertificate
                .Certificate
                .Subject
                .Split('=')
                .LastOrDefault();

            _serviceEndpoint = endpoints
                .Select(x => new EndpointAddress(x.Address, EndpointIdentity.CreateDnsIdentity(subject)))
                .ToList();

            return (TServiceContract)_proxy.GetTransparentProxy();
        }

        public TServiceContract GetHttpProxy(string host, int port)
        {
            _channelFactory = CreateHttpFactory();

            var uriBuilder = new UriBuilder("http", host, port, typeof(TServiceContract).Name);

            var endpoint = new EndpointAddress(uriBuilder.Uri);
            _serviceEndpoint.Add(endpoint);            

            return (TServiceContract)_proxy.GetTransparentProxy();
        }

        public TServiceContract GetSecureHttpProxy(string host, int port, string subject)
        {
            _channelFactory = CreateSecureHttpFactory(subject);

            var uriBuilder = new UriBuilder("http", host, port, typeof(TServiceContract).Name);
            var endpoint = new EndpointAddress(uriBuilder.Uri, EndpointIdentity.CreateDnsIdentity(subject));
            _serviceEndpoint.Add(endpoint);

            return (TServiceContract)_proxy.GetTransparentProxy();
        }

        private ChannelFactory<TServiceContract> CreateHttpFactory()
        {
            return new ChannelFactory<TServiceContract>(new BasicHttpBinding(BasicHttpSecurityMode.None));
        }

        private ChannelFactory<TServiceContract> CreateSecureHttpFactoryFromThumbprint(string thumbprint)
        {
            var httpBinding = new WSHttpBinding(SecurityMode.Message);
            httpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

            var channelFactory = new ChannelFactory<TServiceContract>(httpBinding);
            channelFactory.Credentials.ClientCertificate
                .SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindByThumbprint, thumbprint);

            return channelFactory;
        }

        private ChannelFactory<TServiceContract> CreateSecureHttpFactory(string subject)
        {
            var httpBinding = new WSHttpBinding(SecurityMode.Message);
            httpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;

            var channelFactory = new ChannelFactory<TServiceContract>(httpBinding);
            channelFactory.Credentials.ClientCertificate
                .SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, subject);

            return channelFactory;
        }

        private static object ChannelProxy(MethodBase methodBase, MethodParameterCollection parameterCollection)
        {
            var methodInfo = methodBase as MethodInfo;

            EndpointAddress endpoint;

            lock (_serviceEndpoint)
            {
                endpoint = _serviceEndpoint.OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            }

            var service = _channelFactory.CreateChannel(endpoint);

            object result = null;
            bool success = false;

            try
            {
                result = methodInfo.Invoke(service, parameterCollection.Args);
                ((IClientChannel)service).Close();

                success = true;
            }
            finally
            {
                if (!success)
                {
                    ((IClientChannel)service).Abort();
                }
            }

            return result;
        }
    }
}
