using System;
using System.Collections.Generic;
using System.IO;
using Haipa.IdentityModel.Clients.Internal;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;

namespace Haipa.IdentityModel.Clients
{
    public class LocalIdentityProviderInfo
    {
        protected readonly IEnvironment Environment;
        private readonly string _identityProviderName;

        public LocalIdentityProviderInfo(IEnvironment environment, string identityProviderName = "identity")
        {
            Environment = environment;
            _identityProviderName = identityProviderName;
        }

        public bool IsRunning => GetIsRunning();
        public IReadOnlyDictionary<string,Uri> Endpoints => GetEndpoints();
        protected virtual bool GetIsRunning()
        {

            var metadata = GetMetadata();

            metadata.TryGetValue("processName", out var processName);
            metadata.TryGetValue("processId", out var processId);

            if (string.IsNullOrWhiteSpace(processName?.ToString()) || processId== null)
                return false;

            return Environment.IsProcessRunning((string) processName, Convert.ToInt32(processId));
        }



        private IDictionary<string, object> GetMetadata()
        {
            var applicationDataPath =
                Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "haipa");

            var lockFilePath = Path.Combine(Path.Combine(applicationDataPath,
                $@"{_identityProviderName}{Path.DirectorySeparatorChar}.lock"));

            try
            {
                using var reader = Environment.FileSystem.OpenText(lockFilePath);
                var lockFileData = reader.ReadToEnd();
                return JObject.Parse(lockFileData).ToObject<IDictionary<string, object>>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }

        }

        private IReadOnlyDictionary<string, Uri> GetEndpoints()
        {
            var result = new Dictionary<string, Uri>();
            if (!IsRunning) return result;

            var metadata = GetMetadata();

            if (!metadata.TryGetValue("endpoints", out var endpointsObject)) return result;

            var endpointsJObject = (JObject) endpointsObject;

            foreach (var kv in endpointsJObject)
            {
                result.Add(kv.Key, new Uri(kv.Value.ToString()));
            }

            return result;
        }

        public AsymmetricCipherKeyPair GetSystemClientPrivateKey()
        {
            var applicationDataPath =
                Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), "haipa");

            var privateKeyPath = Path.Combine(applicationDataPath,
                $@"{_identityProviderName}{Path.DirectorySeparatorChar}private{Path.DirectorySeparatorChar}clients{Path.DirectorySeparatorChar}system-client.key");

            return PrivateKeyFile.Read(privateKeyPath, Environment.FileSystem);

        }
    }
}