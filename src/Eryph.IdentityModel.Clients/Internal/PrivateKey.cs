using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace Eryph.IdentityModel.Clients.Internal
{
    public static class PrivateKey
    {
        public static AsymmetricCipherKeyPair ReadFile(string filepath, IFileSystem fileSystem)
        {
            using var reader = new StreamReader(fileSystem.OpenStream(filepath));
            return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();

        }

        public static AsymmetricCipherKeyPair ReadFile(string filepath, IFileSystem fileSystem, PrivateKeyProtectionLevel protectionLevel)
        {
            var scope = protectionLevel switch
            {
                PrivateKeyProtectionLevel.User => DataProtectionScope.CurrentUser,
                PrivateKeyProtectionLevel.Machine => DataProtectionScope.LocalMachine,
                _ => throw new ArgumentOutOfRangeException(nameof(protectionLevel), protectionLevel, null)
            };

            try
            {
                var entropy = Encoding.UTF8.GetBytes(filepath.ToLowerInvariant());
                using var stream = fileSystem.OpenStream(filepath);

                using var protectedDataStream = new MemoryStream();
                stream.CopyTo(protectedDataStream);
                protectedDataStream.Seek(0, SeekOrigin.Begin);

                using var decryptedData = new MemoryStream(
                    ProtectedData.Unprotect(protectedDataStream.GetBuffer(), entropy, scope));
                using var reader = new StreamReader(decryptedData);
                return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
            }
            catch (Exception)
            {
                //ignored
            }

            return null;
        }

        public static AsymmetricCipherKeyPair ReadString(string keyString)
        {
            using var reader = new StringReader(keyString);
            return (AsymmetricCipherKeyPair)new PemReader(reader).ReadObject();
        }

        public static void WriteFile(string filepath, AsymmetricCipherKeyPair keyPair, IFileSystem fileSystem)
        {
            using var writer = new StreamWriter(fileSystem.CreateStream(filepath));
            new PemWriter(writer).WriteObject(keyPair);
        }

        public static void WriteFile(string filepath, AsymmetricCipherKeyPair keyPair, IFileSystem fileSystem, PrivateKeyProtectionLevel protectionLevel)
        {
            var scope = protectionLevel switch
            {
                PrivateKeyProtectionLevel.User => DataProtectionScope.CurrentUser,
                PrivateKeyProtectionLevel.Machine => DataProtectionScope.LocalMachine,
                _ => throw new ArgumentOutOfRangeException(nameof(protectionLevel), protectionLevel, null)
            };

            using var memoryStream = new MemoryStream();
            using var memoryStreamWriter = new StreamWriter(memoryStream);
            var pem = new PemWriter(memoryStreamWriter);
            pem.WriteObject(keyPair);
            memoryStreamWriter.Flush();

            var entropy = Encoding.UTF8.GetBytes(filepath.ToLowerInvariant());
            
            var protectedData = ProtectedData.Protect(
                memoryStream.ToArray(), entropy, scope);

            using var fileStream = fileSystem.CreateStream(filepath);
            fileStream.Write(protectedData, 0, protectedData.Length);
            fileStream.Flush();
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