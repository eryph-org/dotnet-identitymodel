using System;
using System.Linq;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public sealed class EndpointLookup
    {
        private readonly IEnvironment _systemEnvironment;

        public EndpointLookup(IEnvironment systemEnvironment = null)
        {
            _systemEnvironment = systemEnvironment ?? new DefaultEnvironment();
        }


        public Uri GetEndpoint(string endpointName, string configName = "default")
        {
            var endpointFromConfig =  new ConfigStoresReader(_systemEnvironment, configName).GetEndpoints()
                .Where(x => x.Key.Equals(endpointName))
                .Select(x=>x.Value).FirstOrDefault();

            return endpointFromConfig;
        }
    }
    

}