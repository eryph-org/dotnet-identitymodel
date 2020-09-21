using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public class ConfigStoresReader
    {
        private readonly IEnumerable<ConfigStore> _stores;

        public ConfigStoresReader(IEnvironment environment, string configName = ConfigurationNames.Default) : this(
            GetStores(environment, configName), environment, configName)
        {

        }

        public ConfigStoresReader(IEnumerable<ConfigStore> stores, IEnvironment environment, string configName = ConfigurationNames.Default)
        {
            _stores = stores.Where(x=>x.Exists).ToArray();
        }


        public ClientData GetDefaultClient()
        {
            var defaultClientId =
                _stores.Select(x => x.GetDefaultClientId()).FirstOrDefault(x => !string.IsNullOrEmpty(x));
            return GetClientById(defaultClientId);
        }

        public ClientData GetClientById(string clientId)
        {
            var endpoint = GetIdentityEndpoint();
            return _stores.Where(x => x.Exists).SelectMany(x => x.GetClients(endpoint)).FirstOrDefault(x => x.Id == clientId);
        }

        public ClientData GetClientByName(string clientName)
        {
            var endpoint = GetIdentityEndpoint();
            return _stores.Where(x => x.Exists).SelectMany(x => x.GetClients(endpoint)).FirstOrDefault(x => x.Name == clientName);
        }

        public IReadOnlyDictionary<string, Uri> GetEndpoints()
        {
            return _stores.SelectMany(x => x.Endpoints)
                .GroupBy(x => x.Key)
                .ToDictionary(group => group.Key,
                    group => group.First().Value);

        }

        private Uri GetIdentityEndpoint()
        {
            var identityEndpoint = GetEndpoints().Where(kv => kv.Key == EndpointNames.Identity)
                .Select(x => x.Value).FirstOrDefault();

            if (identityEndpoint == null)
                throw new InvalidOperationException("Could not find identity endpoint in configuration.");

            return identityEndpoint;
        }
        
        private static IEnumerable<ConfigStore> GetStores(IEnvironment environment, [NotNull] string configName = ConfigurationNames.Default)
        {
            bool IsDefaultConfig() =>
                configName.Equals(ConfigurationNames.Default, StringComparison.InvariantCultureIgnoreCase);

            if (configName == null) throw new ArgumentNullException(nameof(configName));

            yield return ConfigStore.GetStore(ConfigStoreLocation.CurrentDirectory, environment, configName);
            if (!IsDefaultConfig())
                yield return ConfigStore.GetStore(ConfigStoreLocation.CurrentDirectory, environment);

            yield return ConfigStore.GetStore(ConfigStoreLocation.User, environment, configName);

            if (!IsDefaultConfig())
                yield return ConfigStore.GetStore(ConfigStoreLocation.User, environment);

            yield return ConfigStore.GetStore(ConfigStoreLocation.System, environment, configName);
            if (!IsDefaultConfig())
                yield return ConfigStore.GetStore(ConfigStoreLocation.System, environment);

        }
    }
}