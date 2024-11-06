using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Eryph.IdentityModel.Clients;
using Eryph.IdentityModel.Clients.Internal;
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
        public void Write_Read_KeyPair_with_protection()
        {
            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

            using var memoryStream = new MemoryStream();

            fileSystemMock.Setup(x => x.CreateStream(It.IsAny<string>())).Returns(new WrappedStream(memoryStream));
            fileSystemMock.Setup(x => x.OpenStream(It.IsAny<string>())).Returns(memoryStream);

            var exp = PrivateKey.ReadString(TestData.PrivateKeyFileString);
            PrivateKey.WriteFile("peng", exp, fileSystemMock.Object, PrivateKeyProtectionLevel.User);
            memoryStream.Seek(0, SeekOrigin.Begin);
            var act = PrivateKey.ReadFile("peng", fileSystemMock.Object, PrivateKeyProtectionLevel.User);
            Assert.NotNull(act);
            Assert.Equal(exp.Private, act.Private);
            Assert.Equal(exp.Public, act.Public);
        }

        [Fact]
        public void Read_return_KeyPair()
        {
            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

            using var memoryStream = new MemoryStream();
            using (var streamWriter = new StreamWriter(memoryStream, leaveOpen: true))
            {
                streamWriter.Write(TestData.PrivateKeyFileString);
            }

            memoryStream.Seek(0, SeekOrigin.Begin);

            fileSystemMock.Setup(x => x.OpenStream(It.IsAny<string>())).Returns(memoryStream);

            var keyPair = PrivateKey.ReadFile(null, fileSystemMock.Object);
            Assert.NotNull(keyPair);
        }

        [Fact]
        public void Write_writes_KeyPairFile()
        {
            var kpGenerator = new RsaKeyPairGenerator();

            // certificate strength 2048 bits
            kpGenerator.Init(new KeyGenerationParameters(
                new SecureRandom(new CryptoApiRandomGenerator()), 512));

            using var memoryStream = new MemoryStream();

            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(x => x.CreateStream(It.IsNotNull<string>()))
                .Returns(new WrappedStream(memoryStream)).Verifiable();

            PrivateKey.WriteFile("path", kpGenerator.GenerateKeyPair(), fileSystemMock.Object);

            fileSystemMock.Verify();
            memoryStream.Seek(0, SeekOrigin.Begin);
            var memoryString = new StreamReader(memoryStream).ReadToEnd();

            Assert.NotEmpty(memoryString);
            Assert.StartsWith("-----BEGIN RSA PRIVATE KEY-----", memoryString);
        }


        private class WrappedStream : Stream
        {
            private readonly Stream _innerStream;
            public WrappedStream(Stream innerStream)
            {
                this._innerStream = innerStream;
            }

            public override void Flush()
            {
                _innerStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _innerStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _innerStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _innerStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _innerStream.Write(buffer, offset, count);
            }

            public override bool CanRead => _innerStream.CanRead;

            public override bool CanSeek => _innerStream.CanSeek;

            public override bool CanWrite => _innerStream.CanWrite;

            public override long Length => _innerStream.Length;

            public override long Position
            {
                get => _innerStream.Position;
                set => _innerStream.Position = value;
            }

            
        }

    }
}