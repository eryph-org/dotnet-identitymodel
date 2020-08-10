using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Haipa.IdentityModel.Clients
{
    public class GeneratedClientData
    {
        public string Id { get;  }
        public X509Certificate Certificate { get; }
        public AsymmetricCipherKeyPair KeyPair { get; }

        public GeneratedClientData(string id, X509Certificate certificate, AsymmetricCipherKeyPair keyPair)
        {
            Id = id;

            Certificate = certificate;
            KeyPair = keyPair;
        }
    }
}
