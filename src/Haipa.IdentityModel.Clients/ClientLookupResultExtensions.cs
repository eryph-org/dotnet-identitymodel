using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Haipa.IdentityModel.Clients
{
    public static class ClientLookupResultExtensions
    {
        public static Task<AccessTokenResponse> GetAccessToken(this ClientLookupResult lookupResult,
            IEnumerable<string> scopes, HttpClient httpClient = null)
        {
            return lookupResult.Client.GetAccessToken(lookupResult.IdentityEndpoint, httpClient);
        }
    }
}