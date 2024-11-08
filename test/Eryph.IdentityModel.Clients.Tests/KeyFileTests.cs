using System.IO;
using Eryph.IdentityModel.Clients.Internal;
using FluentAssertions;
using Moq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using Xunit;

namespace Eryph.IdentityModel.Clients.Tests;

public class KeyFileTests
{
    private const string KeyFileName = "test.key";

    [Fact]
    public void Write_Read_KeyPair_with_protection()
    {
        var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

        using var memoryStream = new MemoryStream();

        fileSystemMock.Setup(x => x.CreateStream(It.IsAny<string>())).Returns(new WrappedStream(memoryStream));
        fileSystemMock.Setup(x => x.OpenStream(It.IsAny<string>())).Returns(memoryStream);

        var expected = PrivateKey.ReadString(TestData.PrivateKeyFileString);
        PrivateKey.WriteFile(KeyFileName, expected, fileSystemMock.Object, PrivateKeyProtectionLevel.User);
        memoryStream.Seek(0, SeekOrigin.Begin);
        var actual = PrivateKey.ReadFile(KeyFileName, fileSystemMock.Object, PrivateKeyProtectionLevel.User);

        actual.Should().NotBeNull();
        actual.Private.Should().Be(expected.Private);
        actual.Public.Should().Be(expected.Public);
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
        keyPair.Should().NotBeNull();
    }

    [Fact]
    public void Write_writes_KeyPairFile()
    {
        var kpGenerator = new RsaKeyPairGenerator();
        // Use small key size for faster test execution
        kpGenerator.Init(new KeyGenerationParameters(
            new SecureRandom(new CryptoApiRandomGenerator()), 512));

        using var memoryStream = new MemoryStream();

        var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
        fileSystemMock.Setup(x => x.CreateStream(It.IsNotNull<string>()))
            .Returns(new WrappedStream(memoryStream)).Verifiable();

        PrivateKey.WriteFile(KeyFileName, kpGenerator.GenerateKeyPair(), fileSystemMock.Object);

        fileSystemMock.Verify();
        memoryStream.Seek(0, SeekOrigin.Begin);
        var memoryString = new StreamReader(memoryStream).ReadToEnd();

        memoryString.Should().StartWith("-----BEGIN RSA PRIVATE KEY-----");
    }

    /// <summary>
    /// Wraps the <paramref name="innerStream"/> and prevents
    /// it from being disposed.
    /// </summary>
    private class WrappedStream(Stream innerStream) : Stream
    {
        public override void Flush()
        {
            innerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
        }

        public override bool CanRead => innerStream.CanRead;

        public override bool CanSeek => innerStream.CanSeek;

        public override bool CanWrite => innerStream.CanWrite;

        public override long Length => innerStream.Length;

        public override long Position
        {
            get => innerStream.Position;
            set => innerStream.Position = value;
        }
    }
}
