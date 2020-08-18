using System;
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
            fileSystemMock.Setup(x => x.OpenText(It.IsAny<string>())).Returns(new StringReader(PrivateKeyFileString));

            var keyPair = PrivateKeyFile.Read(null, fileSystemMock.Object);
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
            
            PrivateKeyFile.Write("path", kpGenerator.GenerateKeyPair(), fileSystemMock.Object);
            
            fileSystemMock.Verify();
            Assert.NotEmpty(sb.ToString());
            Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----", sb.ToString());

        }

        private const string PrivateKeyFileString = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQB5mHPDgNW+Nzr+KaeMeGVrdUnh0sPnilsIA91+Zvtec2KhP06o o4uXETf5oOJLsOvsXPcFjkR+wv12YPevOXAxKqbE7/BPU65FQhMWSGK+F+lNbQMb FKLSsoeaFxSMitf97LAVzSMpt8OmJ732kQ7gSQ7rABRC9/q7R960cNY/5rK2xV4C 7a47f2iolxBDUlhZ2rW8Mdh6kywK29/mAjvt9S0VN5Mvov3wSsD6J28iqvJzdy/5 SNveH7P4K3MBleALBRePY4D00P2le4KdqBxs64ydLg+xqBwOp4aIj6mWGDQlc4TC Y32hHTKK4aPp7sZB3V6oSiK/l06ac6vtquDHAgMBAAECggEAE+xIu3W2j84Y2mAU 1c08QNkc2+Vet+dRdwS7G+TftuAM/wKSbsstKflmRH551ZENdtLcnopq6qIkSWsl 6g3tNgEZBheSNk0ttqdW3UXK9/6O+WKtKZi9/OvHkBXMBiMRtMc9KrVL16AGbIkC dQ3bdCBEU3jV2Qssh9cExGfgkuOMkIgGgAgM/Qk6X+io+JW2hpSmrdra4z6ffPST oCVfbQ+QsLPGmybGs0BzhnzFGcACzskVZHB8rFOqZERXbBdnwjbnmmOK96sgESer BL6VsEUllQXY2N/t4QXqpZP5er16uRAEgjkukqGi2nn6Hjs8UhvteyRfU0JQRPnq x1+eoQKBgQDcWKCBAsl/OxidQtCGPzLMuuua/QEdvlofb+ogr7V0aZ1aJi9OxhBJ 3JjuguRovU7p6rbAgr+V+4LsKL3VOO4sildoXavPY2R9Eeuh/y1oNY+dnw0OkEo3 0uBuIYdek8s66yFGDPmbfu3+tzeF7fu6+G/eRSsX9LBXExBb24OxXwKBgQCNRUm+ b4Lm1Yfya9qju/ciDD0g4rlRc/MtzSaovLBxcTMFIrUguYyOXEkYiOWUbPZEvIk3 ZsGzb+Iv5lhn2JvI0z8Rh5BfPdYrgbAbEPiemKgJQs2pMGpds8vGpDFGCHnzHJko MolJRbOsRER+lMhnbCI5B9zx6W0/i1dNj4uBmQKBgQCWsu6jDVrt72b4NzgSeKqv pq94gsz+oK9WjN4dmM6LXahGfZMhVwjQ21Sk21SH5eFQzjxLEaEiXK/AAGVErPkH 8V2yfU4COsIBX/49/x35BZjBfoQZj8mSwGDKMZg5sO7vztwk4r7cAEWZTYllycu+ picsZzX/3lO0Wc94Y3uAFQKBgG4uPifC/QtgOxl9uRa+wS7S8NI3QmYe0ulD+gTc tZikuzAkM7SEQvW9UF1MWBJ9MU3G5hZJlIWIm5bURtsne8kTyTq4yocdyW5BRcK2 Z9H6KgSfD5wHYM4YLrSM1slSTxqnkWRileSJ8mpHDEzVacAP/FkSouYiMsy+tqaN cDbxAoGBAKtsRWhjxeABPpwdEFFuFyDcoCGb8MIKHiob4AXrNP0OqwecH7DMVvQf mFcM1j8PydHzHFqJXjS0qVhxRPhODBnS1d2lTwvrM7Cqmrgd7QUpZMKNb7rGGSf4 1ruNA/IDbxQURAYjZBL7IGzmvLt+nAyZMd08B9ACyk2D3gtXDIdz 
-----END RSA PRIVATE KEY-----";
    }
}