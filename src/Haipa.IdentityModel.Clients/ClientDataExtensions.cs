using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public static class ClientDataExtensions
    {
        public static Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, HttpClient httpClient = null)
        {
            return clientData.GetAccessToken(null, httpClient);
        }

        public static async Task<AccessTokenResponse> GetAccessToken(this ClientData clientData,
            IEnumerable<string> scopes, HttpClient httpClient = null)
        {
            var disposeHttpClient = httpClient == null;
            httpClient ??= new HttpClient();

            httpClient.BaseAddress = clientData.IdentityProvider;

            if (!disposeHttpClient)
                return await httpClient.GetClientAccessToken(
                    clientData.ClientName,
                    clientData.KeyPair.ToRSAParameters(), scopes).ConfigureAwait(false);

            using (httpClient)
            {
                var result = await httpClient.GetClientAccessToken(
                    clientData.ClientName,
                    clientData.KeyPair.ToRSAParameters(), scopes).ConfigureAwait(false);
                return result;
            }
        }
    }
}