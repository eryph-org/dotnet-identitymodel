using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public sealed class ClientData
    {

        public ClientData(string id, string name, AsymmetricCipherKeyPair keyPair, Uri identityProvider, string configurationName)
        {
            Id = id;
            Name = name;
            KeyPair = keyPair;
            IdentityProvider = identityProvider;
            ConfigurationName = configurationName;
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal ClientData()
        {}


        [DataMember]
        public string Id { get; }


        [DataMember]
        public string Name { get; }

        [JsonIgnore]
        public AsymmetricCipherKeyPair KeyPair { get; }

        [JsonIgnore]
        public Uri IdentityProvider { get; }

        [JsonIgnore]
        public string ConfigurationName { get; set; }

        public Task<AccessTokenResponse> GetAccessToken(HttpClient httpClient = null)
        {
            return GetAccessToken(null, httpClient);
        }

        public async Task<AccessTokenResponse> GetAccessToken(IEnumerable<string> scopes, HttpClient httpClient = null)
        {
            var disposeHttpClient = httpClient == null;
            httpClient ??= new HttpClient();

            httpClient.BaseAddress = IdentityProvider;

            if (!disposeHttpClient)
                return await httpClient.GetClientAccessToken(
                    Id,
                    KeyPair.ToRSAParameters(), scopes).ConfigureAwait(false);

            using (httpClient)
            {
                var result = await httpClient.GetClientAccessToken(
                    Id,
                    KeyPair.ToRSAParameters(), scopes).ConfigureAwait(false);
                return result;
            }
        }

    }
}