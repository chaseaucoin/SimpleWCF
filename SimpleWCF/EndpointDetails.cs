using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading.Tasks;

namespace SimpleWCF
{
    [DataContract]

    public class EndpointDetails

    {
        public EndpointDetails()
        { }

        [DataMember]
        public Uri Address { get; set; }
        
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Binding { get; set; }

        [DataMember]
        public string SecurityMode { get; set; }

        [DataMember]
        public string ThumbPrint { get; set; }

        [DataMember]
        public string ContractShortName { get; set; }

        [DataMember]
        public string ContractFullName { get; set; }
    }
}
