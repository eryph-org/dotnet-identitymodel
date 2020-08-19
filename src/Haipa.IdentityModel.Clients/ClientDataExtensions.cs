using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Haipa.IdentityModel.Clients
{
    public static class ClientDataExtensions
    {
        public static Task<AccessTokenResponse> GetAccessToken(this ClientData clientData, string identityEndpoint, HttpMessageHandler handler = null)
        {
            var httpClient = handler == null
                ? new HttpClient()
                : new HttpClient(handler);

            httpClient.BaseAddress = new Uri(identityEndpoint);

            return httpClient.GetClientAccessToken(
                clientData.ClientName,
                clientData.KeyPair.ToRSAParameters());

        }
    }
}
