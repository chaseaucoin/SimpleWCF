using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleWCF.Discovery
{
    public class DiscoveryClient : IDisposable
    {
        HubConnection _hubConnection;

        IHubProxy _discoveryProxy;
        
        public DiscoveryClient(int port)
        {
            _hubConnection = new HubConnection(string.Format("http://localhost:{0}/", port));
            _discoveryProxy = _hubConnection.CreateHubProxy("DiscoveryHub");

            _discoveryProxy.On<IEnumerable<EndpointDetails>>("UpdateEndpoints", endpoint =>
                UpdateEndpoints(endpoint)
            );

            _discoveryProxy.On("CheckHealth", () =>{
                if (CheckHealth != null)
                    CheckHealth();
            });

            _hubConnection.Start().Wait();
        }

        public event Action<IEnumerable<EndpointDetails>> UpdateEndpoints;

        public event Action CheckHealth;
        
        public void AddService(EndpointDetails endpoint)
        {
            _discoveryProxy.Invoke("AddService", endpoint).Wait();
        }

        public IEnumerable<EndpointDetails> ListenForEndpoints<TServiceContract>()
        {
            var output = _discoveryProxy.Invoke<IEnumerable<EndpointDetails>>("ListenForEndpoints", typeof(TServiceContract).FullName)
                .Result;

            return output;
        }

        public void Dispose()
        {
            _hubConnection.Dispose();
        }
    }
}
