using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace Haipa.IdentityModel.Clients
{
    public class GeneratedClientData : ClientData
    {
        public X509Certificate Certificate { get; }

        public GeneratedClientData(string clientName, X509Certificate certificate, AsymmetricCipherKeyPair keyPair) : base(clientName, keyPair)
        {
            Certificate = certificate;
        }
    }
}
