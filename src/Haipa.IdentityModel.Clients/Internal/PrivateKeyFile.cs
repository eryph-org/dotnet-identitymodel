using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Haipa.IdentityModel.Clients.Internal
{
    public static class PrivateKeyFile
    {
        
        public static AsymmetricCipherKeyPair Read(string filepath, IFileSystem fileSystem)
        {

            using (var reader = fileSystem.OpenText(filepath))
                return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

        }

        public static void Write(string filepath, AsymmetricCipherKeyPair keyPair, IFileSystem fileSystem)
        {
            using (var writer = fileSystem.CreateText(filepath))
                new PemWriter(writer).WriteObject(keyPair);

        }
    }
}