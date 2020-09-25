using System.IO;
using System.Security;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Haipa.IdentityModel.Clients.Internal
{
    public static class PrivateKey
    {
        public static AsymmetricCipherKeyPair ReadFile(string filepath, IFileSystem fileSystem)
        {
            using var reader = fileSystem.OpenText(filepath);
            return (AsymmetricCipherKeyPair) new PemReader(reader).ReadObject();
        }

        public static AsymmetricCipherKeyPair ReadString(string keyString)
        {
            using var reader = new StringReader(keyString);
            return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
        }

        public static void WriteFile(string filepath, AsymmetricCipherKeyPair keyPair, IFileSystem fileSystem)
        {
            using var writer = fileSystem.CreateText(filepath);
            new PemWriter(writer).WriteObject(keyPair);
        }

        public static SecureString ToSecureString(AsymmetricCipherKeyPair keyPair)
        {
            var privateKeyStringBuilder = new StringBuilder();

            using (var writer = new StringWriter(privateKeyStringBuilder))
            {
                new PemWriter(writer).WriteObject(keyPair);
            }

            var result = new SecureString();
            foreach (var c in privateKeyStringBuilder.ToString())
            {
                result.AppendChar(c);
            }
            result.MakeReadOnly();

            return result;
        }


    }
}