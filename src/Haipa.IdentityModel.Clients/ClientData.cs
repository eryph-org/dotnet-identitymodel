using Org.BouncyCastle.Crypto;

namespace Haipa.IdentityModel.Clients
{
    public class ClientData
    {
        public ClientData(string clientName,  AsymmetricCipherKeyPair keyPair)
        {
            ClientName = clientName;
            KeyPair = keyPair;
        }

        public string ClientName { get;  }
        public AsymmetricCipherKeyPair KeyPair { get; }

    }

    public class ClientLookupResult
    {
        public bool IsHaipaZero { get; set; }
        public ClientData Client { get; set; }
        public string IdentityEndpoint { get; set; }
        public string ApiEndpoint { get; set; }

    }
}