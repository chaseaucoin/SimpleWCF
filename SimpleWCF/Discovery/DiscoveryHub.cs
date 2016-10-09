using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SimpleWCF.Discovery
{
    public class DiscoveryHub : Hub
    {
        static ConcurrentDictionary<Uri, EndpointDetails> _endpointDetails = new ConcurrentDictionary<Uri, EndpointDetails>();
        
        public void AddService(EndpointDetails endPoint)
        {
            var ip = GetIpAddress();
            var hostName = Dns.GetHostEntry(ip).HostName;

            var uriString = endPoint.Address.ToString().Replace("0.0.0.0", hostName);
            endPoint.Address = new Uri(uriString);

            _endpointDetails.AddOrUpdate(endPoint.Address, endPoint, (uri, oldEndpoint) => endPoint);
            var contractEndpoints = _endpointDetails
                .Where(x => x.Value.ContractFullName == endPoint.ContractFullName)
                .Select(x => x.Value);

            Groups.Add(Context.ConnectionId, "Services");

            Clients
                .Group(endPoint.ContractFullName)
                .UpdateEndpoints(contractEndpoints);
        }

        public IEnumerable<EndpointDetails> ListenForEndpoints(string contractFullName)
        {
            Groups.Add(Context.ConnectionId, contractFullName);

            return _endpointDetails
                .Where(x => x.Value.ContractFullName == contractFullName)
                .Select(x => x.Value);
        }

        protected string GetIpAddress()
        {
            return (string) Context.Request.Environment["server.RemoteIpAddress"];
        }
    }
}
