using System;
using System.IO;
using System.Runtime.InteropServices;
using Haipa.IdentityModel.Clients;
using Moq;
using Xunit;

namespace IdentityModel.Clients.Tests
{
    public class SystemClientLookupTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSystemClient_returns_null_if_process_not_running(bool forHaipaZero)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);

            var moduleName = forHaipaZero ? "zero" : "identity";
            filesystemMock.Setup(x =>
                    x.OpenText(It.Is<string>(p => p.EndsWith($"{moduleName}{Path.DirectorySeparatorChar}.run_info"))))
                .Returns(new StringReader("{\"process_id\" : 100, \"url\" : \"http://haipa.io\"}"));


            environmentMock.Setup(x => x.IsProcessRunning("", 100)).Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Null( clientLookup.GetSystemClient());
        }


        [Theory]
        [InlineData("http://haipa.io/identity", "http://haipa.io/identity")]
        [InlineData("http://haipa.io", "http://haipa.io/")]
        [InlineData("http://localhost:4711", "http://localhost:4711/")]
        public void GetSystemClient_reads_process_info(string baseUrl, string identityEndpoint)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);
            

            filesystemMock.Setup(x =>
                    x.OpenText(It.Is<string>(p => p.EndsWith(".lock"))))
                .Returns( () => new StringReader($"{{\"processName\":\"TestingHaipa\",\"processId\":100,\"endpoints\":{{\"identity\":\"{baseUrl}\"}}}}"));

            environmentMock.Setup(x => x.IsProcessRunning("TestingHaipa", 100)).Returns(true);

            var clientLookup = new ClientLookup(environmentMock.Object);
            var response = clientLookup.GetSystemClient();

            Assert.NotNull(response);
            Assert.Equal(identityEndpoint, response.IdentityProvider.ToString());
            Assert.NotNull(response.KeyPair);
            Assert.Equal("system-client", response.Id);

            environmentMock.Verify();
            filesystemMock.Verify();
        }

        private Mock<IFileSystem> SetupEnvironmentAndFileSystemWithClientKey(Mock<IEnvironment> environmentMock)
        {
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(false);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux))).Returns(true);

            var fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileSystemMock.Setup(x => x.OpenText(It.IsAny<string>())).Throws<FileNotFoundException>();

            fileSystemMock.Setup(x => x.OpenText(It.Is<string>(x => x.EndsWith("system-client.key"))))
                .Returns(new StringReader(TestData.PrivateKeyFileString));

            environmentMock.Setup(x => x.FileSystem).Returns(fileSystemMock.Object);
            return fileSystemMock;
        }

        [Fact]
        public void GetSystemClient_Throws_if_not_AdminOn_Windows()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(true);
            environmentMock.Setup(x => x.IsWindowsAdminUser).Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() =>clientLookup.GetSystemClient());
        }

        [Fact]
        public void GetSystemClient_Throws_if_OSX()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux || p == OSPlatform.Windows)))
                .Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() => clientLookup.GetSystemClient());
            environmentMock.Verify();
        }

        [Fact]
        public void GetSystemClient_Throws_if_HaipaZero_and_not_Windows()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows)))
                .Returns(false);
            environmentMock
                .Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Linux)))
                .Returns(true);


            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(() => clientLookup.GetSystemClient("zero"));
            environmentMock.Verify();
        }
    }
}