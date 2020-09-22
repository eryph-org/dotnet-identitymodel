using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Haipa.IdentityModel.Clients.Internal;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public class ConfigStore
    {
        private readonly string _configName;
        private readonly IEnvironment _environment;

        private ConfigStore(string configName, string basePath, IEnvironment environment)
        {
            _configName = configName;
            _environment = environment;

            StorePath = ".haipa";
            if (basePath != null)
                StorePath = Path.Combine(basePath, StorePath);
        }

        public string StorePath { get; }

        private ClientConfig _config;
        private object _syncRoot = new object();

        public IReadOnlyDictionary<string, Uri> Endpoints => 
            new ReadOnlyDictionary<string, Uri>(GetSettings().Endpoints??new Dictionary<string, Uri>());

        public bool Exists => _environment.FileSystem.DirectoryExists(StorePath) 
                              && _environment.FileSystem.FileExists(Path.Combine(StorePath, $"{_configName}.config"));

        public IEnumerable<ClientData> GetClients(Uri identityEndpoint = null)
        {
            return from client in (GetSettings().Clients ?? new ClientData[0]) 
                let keyFileName = Path.Combine(StorePath, "private", $"{client.Id}.key") 
                where _environment.FileSystem.FileExists(keyFileName) 
                let privateKey = PrivateKeyFile.Read(keyFileName, _environment.FileSystem) 
                select new ClientData(client.Id, client.Name, privateKey, identityEndpoint, _configName);
        }


        private ClientConfig GetSettings()
        {
            lock (_syncRoot)
            {
                var configFileName = Path.Combine(StorePath, $"{_configName}.config");
                if (_environment.FileSystem.FileExists(configFileName))
                {
                    using var reader = _environment.FileSystem.OpenText(configFileName);
                    var configJson = reader.ReadToEnd();
                    _config = JsonConvert.DeserializeObject<ClientConfig>(configJson);
                }
                else
                {
                    _config = new ClientConfig();
                }

                return _config;
            }
        }

        
        private void SaveSettings(ClientConfig settings)
        {
            lock (_syncRoot)
            {
                if (!_environment.FileSystem.DirectoryExists(StorePath))
                    Directory.CreateDirectory(StorePath);

                var configFileName = Path.Combine(StorePath, $"{_configName}.config");

                var settingsJson = JsonConvert.SerializeObject(settings, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()

                });
                using var writer = _environment.FileSystem.CreateText(configFileName);
                writer.Write(settingsJson);
                _config = null;
            }
        }

        internal string GetDefaultClientId()
        {
            var settings = GetSettings();
            return settings.DefaultClientId;
        }

        internal void SetDefaultClientId(string clientId)
        {
            var settings = GetSettings();
            settings.DefaultClientId = clientId;
            SaveSettings(settings);

        }

        public void SetEndpoint(string endpointName, Uri endpoint)
        {
            var settings = GetSettings();

            settings.Endpoints ??= new Dictionary<string, Uri>();

            if (settings.Endpoints.ContainsKey(endpointName))
                settings.Endpoints.Remove(endpointName);

            

            settings.Endpoints.Add(EndpointNames.Identity, endpoint);
            SaveSettings(settings);
        }

        public void AddClient([NotNull] ClientData client, [NotNull] ConfigStore endpointConfigStore)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (endpointConfigStore == null) throw new ArgumentNullException(nameof(endpointConfigStore));

            if (client.ConfigurationName != _configName)
                throw new InvalidOperationException($"client '{client.Id}' has been created for configuration '{client.ConfigurationName}'.");


            if (client.Id == "system-client")
                throw new InvalidOperationException("The system client cannot be saved to config store.");

            if (_configName != ConfigurationNames.Zero && _configName != ConfigurationNames.Local)
            {
                endpointConfigStore.Endpoints.TryGetValue(EndpointNames.Identity, out var currentEndpoint);

                if (currentEndpoint == null)
                {
                    currentEndpoint = client.IdentityProvider;
                    endpointConfigStore.SetEndpoint(EndpointNames.Identity, currentEndpoint);
                }

                if (client.IdentityProvider != currentEndpoint)
                    throw new InvalidOperationException($"This client has been issued by identity provider '{client.IdentityProvider}', but the current configuration uses the provider '{currentEndpoint}'.");

            }
            
            var settings = GetSettings();
            settings.Clients ??= new List<ClientData>();

            var privatePath = Path.Combine(StorePath, "private");

            var privateKeyPath = Path.Combine(privatePath, $"{client.Id}.key");
            if (!_environment.FileSystem.DirectoryExists(privatePath))
                _environment.FileSystem.CreateDirectory(privatePath);
            
            var clients = new List<ClientData>(settings.Clients.Where(x => x.Id != client.Id)) {client};


            PrivateKeyFile.Write(privateKeyPath, client.KeyPair, _environment.FileSystem);
            settings.Clients = clients;
            SaveSettings(settings);
        }

        public static ConfigStore GetStore(ConfigStoreLocation location, IEnvironment environment, [NotNull] string configName = ConfigurationNames.Default)
        {
            if (configName == null) throw new ArgumentNullException(nameof(configName));

            var basePath = location switch
            {
                ConfigStoreLocation.CurrentDirectory => environment.FileSystem.GetCurrentDirectory(),
                ConfigStoreLocation.User => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ConfigStoreLocation.System => Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                _ => throw new ArgumentOutOfRangeException(nameof(location), location, null)
            };

            return new ConfigStore(configName, basePath, environment);
        }
    }
}

