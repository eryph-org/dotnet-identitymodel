using System;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public class ConfigStoresWriter
    {
        private readonly ConfigStoresWriterSettings _settings;


        public ConfigStoresWriter(IEnvironment environment, string configName) : this(ConfigStoresWriterSettings.DefaultSettings(environment, configName))
        {
        }

        public ConfigStoresWriter(ConfigStoresWriterSettings settings)
        {
            _settings = settings;
        }


        public void SetDefaultClient([NotNull] ClientData client)
        {
            AddClient(client);
            _settings.DefaultsStore.SetDefaultClientId(client.Id);
        }

        public void AddClient([NotNull] ClientData client)
        {
            _settings.ClientsStore.AddClient(client, _settings.EndpointsStore);
        }

        public void AddEndpoint(string endpointName, Uri endpoint)
        {
            _settings.EndpointsStore.SetEndpoint(endpointName, endpoint);
        }
    }
}