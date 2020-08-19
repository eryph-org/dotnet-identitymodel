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
            var token = await result.Client.GetAccessToken(result.IdentityEndpoint);

            Console.WriteLine(token.AccessToken);
        }
    }
}
