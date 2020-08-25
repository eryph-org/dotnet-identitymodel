using System;
using System.Runtime.Serialization;
using Org.BouncyCastle.Crypto;

namespace Haipa.IdentityModel.Clients
{
    public sealed class ClientData
    {
        public ClientData(string clientName, AsymmetricCipherKeyPair keyPair, Uri identityProvider)
        {
            ClientName = clientName;
            KeyPair = keyPair;
            IdentityProvider = identityProvider;
        }

        [DataMember]
        public string ClientName { get; }

        [DataMember]
        public AsymmetricCipherKeyPair KeyPair { get; }

        [DataMember]
        public Uri IdentityProvider { get; }
    }
}