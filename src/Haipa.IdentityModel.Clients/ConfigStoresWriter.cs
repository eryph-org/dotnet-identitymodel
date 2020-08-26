using System;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    public class ConfigStoresWriter
    {
        private readonly IEnvironment _environment;
        private readonly string _configName;


        public ConfigStoresWriter(IEnvironment environment, string configName = "default")
        {
            _environment = environment;
            _configName = configName;
        }

        public void SetDefaultClient([NotNull] ClientData client)
        {
            AddClient(client);
            ConfigStore.GetDefaultStore(_environment, ConfigStoreContent.Defaults, _configName)
                .SetDefaultClientId(client.Id);
        }

        public void AddClient([NotNull] ClientData client)
        {
            ConfigStore.GetDefaultStore(_environment, ConfigStoreContent.Clients, _configName)
                .AddClient(client,
                    ConfigStore.GetDefaultStore(_environment, ConfigStoreContent.Endpoints, _configName));
        }

        public void AddEndpoint(string endpointName, Uri endpoint)
        {
            ConfigStore.GetDefaultStore(_environment, ConfigStoreContent.Endpoints, _configName)
                .SetEndpoint(endpointName, endpoint);
        }
    }
}