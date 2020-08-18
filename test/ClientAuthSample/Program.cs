using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Security;
using System.Threading.Tasks;
using Haipa.IdentityModel.Clients;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace ClientAuthSample
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientLockup = new ClientLookup(new DefaultEnvironment());
            var result = clientLockup.GetClient();
            var httpClient = new HttpClient{ BaseAddress = new Uri(result.IdentityEndpoint) };

            var token = await httpClient.GetClientAccessToken(
                    result.Client.ClientName, 
                    result.Client.KeyPair.ToRSAParameters())
                .ConfigureAwait(false);

            Console.WriteLine(token.AccessToken);
        }
    }
}
