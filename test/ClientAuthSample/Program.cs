using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Haipa.IdentityModel.Clients;

namespace ClientAuthSample
{
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        private static async Task Main()
        {
            var clientLockup = new ClientLookup(new DefaultEnvironment());

            var client = clientLockup.GetDefaultClient("zero");
            
            if (client != null)
            {
                var token = await client.GetAccessToken();

                Console.WriteLine(token.AccessToken);
            }
            else
            {
                Console.WriteLine("could not find any Haipa client");
            }
        }
    }
}