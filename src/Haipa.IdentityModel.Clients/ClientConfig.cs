using System;
using System.Collections.Generic;

namespace Haipa.IdentityModel.Clients
{
    public class ClientConfig
    {

        public string DefaultClientId { get; set; }


        public IEnumerable<ClientData> Clients { get; set; }

        public IDictionary<string, Uri> Endpoints { get; set; }
    }
}