using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Haipa.IdentityModel.Clients
{
    public static class ClientDataExtensions
    {
        public static Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, string identityEndpoint, HttpClient httpClient = null)
        {
            return clientData.GetAccessToken(identityEndpoint, null, httpClient);
        }

        public static Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, string identityEndpoint, IEnumerable<string> scopes, HttpClient httpClient = null)
        {
            var disposeHttpClient = httpClient == null;
            httpClient ??= new HttpClient();

            httpClient.BaseAddress = new Uri(identityEndpoint);

            if (!disposeHttpClient)
                return httpClient.GetClientAccessToken(
                    clientData.ClientName,
                    clientData.KeyPair.ToRSAParameters(), scopes);

            using(httpClient)
                return httpClient.GetClientAccessToken(
                    clientData.ClientName,
                    clientData.KeyPair.ToRSAParameters(), scopes);

        }
    }
}
