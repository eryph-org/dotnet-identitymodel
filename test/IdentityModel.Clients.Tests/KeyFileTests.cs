using System.IO;
using System.Text;
using Haipa.IdentityModel.Clients;
using Haipa.IdentityModel.Clients.Internal;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Xunit;

namespace IdentityModel.Clients.Tests
{
    public class KeyFileTests
    {


        [Fact]
        public void Read_return_KeyPair()
        {
            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(x => x.OpenText(It.IsAny<string>())).Returns(new StringReader(TestData.PrivateKeyFileString));

            var keyPair = PrivateKey.ReadFile(null, fileSystemMock.Object);
            Assert.NotNull(keyPair);
        }

        [Fact]
        public void Write_writes_KeyPairFile()
        {
            //var (certificate, keyPair) = X509Generation.GenerateSelfSignedCertificate("test-client");
            //var certString = Convert.ToBase64String(certificate.GetEncoded());


            var kpGenerator = new RsaKeyPairGenerator();

            // certificate strength 2048 bits
            kpGenerator.Init(new KeyGenerationParameters(
                new SecureRandom(new CryptoApiRandomGenerator()), 512));

            var sb = new StringBuilder();
            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(x => x.CreateText(It.IsNotNull<string>())).Returns(new StringWriter(sb)).Verifiable();

            PrivateKey.WriteFile("path", kpGenerator.GenerateKeyPair(), fileSystemMock.Object);

            fileSystemMock.Verify();
            Assert.NotEmpty(sb.ToString());
            Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----", sb.ToString());
        }
    }
}