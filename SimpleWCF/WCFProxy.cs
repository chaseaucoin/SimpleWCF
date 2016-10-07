using CupcakeFactory.SimpleProxy;
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
        ChannelFactory<TServiceContract> _channelFactory;
        SimpleProxy<TServiceContract> _proxy;
        EndpointAddress _serviceEndpoint;

        public WCFProxy()
        {
            _proxy = new SimpleProxy<TServiceContract>(ChannelProxy);
        }

        public TServiceContract GetHttpProxy(string host, int port)
        {
            _channelFactory = new ChannelFactory<TServiceContract>(new BasicHttpBinding(BasicHttpSecurityMode.None));
            var uriBuilder = new UriBuilder("http", host, port, typeof(TServiceContract).Name);
            _serviceEndpoint = new EndpointAddress(uriBuilder.Uri);

            return (TServiceContract)_proxy.GetTransparentProxy();
        }

        public TServiceContract GetSecureHttpProxy(string host, int port, string subject)
        {
            var httpBinding = new WSHttpBinding(SecurityMode.Message);
            httpBinding.Security.Message.ClientCredentialType = MessageCredentialType.Certificate;
            
            _channelFactory = new ChannelFactory<TServiceContract>(httpBinding);
            _channelFactory.Credentials.ClientCertificate
                .SetCertificate(StoreLocation.LocalMachine, StoreName.TrustedPeople, X509FindType.FindBySubjectName, subject);            
            
            var uriBuilder = new UriBuilder("http", host, port, typeof(TServiceContract).Name);
            _serviceEndpoint = new EndpointAddress(uriBuilder.Uri, EndpointIdentity.CreateDnsIdentity(subject));

            return (TServiceContract)_proxy.GetTransparentProxy();
        }

        private object ChannelProxy(MethodBase methodBase, MethodParameterCollection parameterCollection)
        {
            var methodInfo = methodBase as MethodInfo;

            var service = _channelFactory.CreateChannel(_serviceEndpoint);

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
