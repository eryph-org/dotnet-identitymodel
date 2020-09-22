using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public sealed class ClientLookup
    {
        private readonly IEnvironment _systemEnvironment;

        public ClientLookup(IEnvironment systemEnvironment = null)
        {
            _systemEnvironment = systemEnvironment ?? new DefaultEnvironment();
        }

        [CanBeNull]
        public ClientData FindClient()
        {
            return _systemEnvironment.IsOsPlatform(OSPlatform.Windows) 
                ? FindClient(ConfigurationNames.Default, ConfigurationNames.Zero, ConfigurationNames.Local) 
                : FindClient(ConfigurationNames.Default, ConfigurationNames.Local);
        }

        [CanBeNull]
        public ClientData FindClient([NotNull] params string[] configNames)
        {
            foreach (var configName in configNames)
            {
                ClientData result;
                try
                {
                    result = GetDefaultClient(configName);
                    if (result != null) return result;

                }
                catch (InvalidOperationException)
                {

                }

                try
                {
                    result = GetSystemClient(configName);
                    if (result != null) return result;

                }
                catch (InvalidOperationException)
                {

                }
            }

            return null;
        }

        [CanBeNull]
        public ClientData GetDefaultClient([NotNull] string configName = ConfigurationNames.Default)
        {
            if (configName == null) throw new ArgumentNullException(nameof(configName));

            return new ConfigStoresReader(_systemEnvironment, configName).GetDefaultClient();
        }

        [CanBeNull]
        public ClientData GetClientByName([NotNull] string clientName, [NotNull] string configName = ConfigurationNames.Default)
        {
            if (clientName == null) throw new ArgumentNullException(nameof(clientName));
            if (configName == null) throw new ArgumentNullException(nameof(configName));

            return new ConfigStoresReader(_systemEnvironment, configName).GetClientByName(clientName);

        }

        [CanBeNull]
        public ClientData GetClientById([NotNull] string clientId, [NotNull] string configName = ConfigurationNames.Default)
        {
            if (clientId == null) throw new ArgumentNullException(nameof(clientId));
            if (configName == null) throw new ArgumentNullException(nameof(configName));

            return new ConfigStoresReader(_systemEnvironment, configName).GetClientById(clientId);
        }

        [CanBeNull]
        public ClientData GetSystemClient(string configName = ConfigurationNames.Local)
        {
            if(configName != ConfigurationNames.Zero && configName != ConfigurationNames.Local)
                throw new InvalidOperationException($"The system client is not supported for configuration '{configName}.");

            if (!_systemEnvironment.IsOsPlatform(OSPlatform.Windows) &&
                !_systemEnvironment.IsOsPlatform(OSPlatform.Linux))
                throw new InvalidOperationException("The system client exists only on Windows and Linux systems.");

            if (!_systemEnvironment.IsOsPlatform(OSPlatform.Windows) && configName == ConfigurationNames.Zero)
                throw new InvalidOperationException("The system client for Haipa zero exists only on Windows.");

            if (_systemEnvironment.IsOsPlatform(OSPlatform.Windows) && !_systemEnvironment.IsWindowsAdminUser)
                throw new InvalidOperationException(
                    "This application has to be started as admin to access the Haipa system client. ");

            
            var identityInfo = configName == ConfigurationNames.Zero
                ? new HaipaZeroInfo(_systemEnvironment)
                : new LocalIdentityProviderInfo(_systemEnvironment);

            if (!identityInfo.IsRunning) return null;

            var identityEndpoint = !identityInfo.Endpoints.TryGetValue(EndpointNames.Identity, out var endpoint)
                ? throw new InvalidOperationException("could not find identity endpoint for system-client")
                : endpoint;

            return new ClientData("system-client", null,
                identityInfo.GetSystemClientPrivateKey(),
                identityEndpoint, configName);

        }

    }
}