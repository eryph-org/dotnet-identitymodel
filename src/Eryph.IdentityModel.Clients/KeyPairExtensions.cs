using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Eryph.IdentityModel.Clients
{
    public static class KeyPairExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static RSAParameters ToRSAParameters(this AsymmetricCipherKeyPair keyPair)
        {
            return DotNetUtilities.ToRSAParameters(keyPair.Private as RsaPrivateCrtKeyParameters);
        }
    }
}