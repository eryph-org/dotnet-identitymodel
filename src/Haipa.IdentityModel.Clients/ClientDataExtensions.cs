using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Haipa.IdentityModel.Clients
{
    public static class ClientDataExtensions
    {
        public static Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, string identityEndpoint, HttpClient httpClient = null)
        {
            return clientData.GetAccessToken(identityEndpoint, null, httpClient);
        }

        public static async Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, string identityEndpoint, IEnumerable<string> scopes, HttpClient httpClient = null)
        {
            var disposeHttpClient = httpClient == null;
            httpClient ??= new HttpClient();

            httpClient.BaseAddress = new Uri(identityEndpoint);

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
