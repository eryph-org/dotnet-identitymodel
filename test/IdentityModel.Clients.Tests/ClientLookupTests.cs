using System;
using System.IO;
using System.Runtime.InteropServices;
using Haipa.IdentityModel.Clients;
using Moq;
using Xunit;

namespace IdentityModel.Clients.Tests
{
    public class ClientLookupTests
    {
        [Fact]
        public void GetSystemClient_Throws_if_OSX()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p=>p == OSPlatform.Linux || p == OSPlatform.Windows ))).Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);
            
            Assert.Throws<InvalidOperationException>(clientLookup.GetSystemClient);
            environmentMock.Verify();
        }

        [Fact]
        public void GetSystemClient_Throws_if_not_AdminOn_Windows()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            environmentMock.Setup(x => x.IsOsPlatform(It.Is<OSPlatform>(p => p == OSPlatform.Windows))).Returns(true);
            environmentMock.Setup(x => x.IsWindowsAdminUser).Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(clientLookup.GetSystemClient);

        }

        [Fact]
        public void GetSystemClient_reads_Private_Key()
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            SetupEnvironmentAndFileSystemWithClientKey(environmentMock);

            var clientLookup = new ClientLookup(environmentMock.Object);


            Assert.Throws<InvalidOperationException>(clientLookup.GetSystemClient);
            environmentMock.Verify();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetSystemClient_throws_if_process_not_running(bool forHaipaZero)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);

            var moduleName = forHaipaZero ? "zero" : "identity";
            filesystemMock.Setup(x => x.OpenText(It.Is<string>(p => p.EndsWith($"{moduleName}{Path.DirectorySeparatorChar}.run_info"))))
                .Returns(new StringReader($"{{\"process_id\" : 100, \"url\" : \"http://haipa.io\"}}"));
            


            environmentMock.Setup(x => x.IsProcessRunning(100)).Returns(false);

            var clientLookup = new ClientLookup(environmentMock.Object);

            Assert.Throws<InvalidOperationException>(clientLookup.GetSystemClient);

        }


        [Theory]
        [InlineData(true, "http://haipa.io","http://haipa.io/identity", "http://haipa.io/api")]
        [InlineData(false, "http://haipa.io", "http://haipa.io", null)]
        [InlineData(false, "http://localhost:4711", "http://localhost:4711", null)]
        public void GetSystemClient_reads_process_info(bool forHaipaZero, string baseUrl, string identityEndpoint, string apiEndpoint)
        {
            var environmentMock = new Mock<IEnvironment>(MockBehavior.Strict);
            var filesystemMock = SetupEnvironmentAndFileSystemWithClientKey(environmentMock);


            var moduleName = forHaipaZero ? "zero" : "identity";

            filesystemMock.Setup(x => x.OpenText(It.Is<string>(p => p.EndsWith($"{moduleName}{Path.DirectorySeparatorChar}.run_info"))))
                .Returns(new StringReader($"{{\"process_id\" : 100, \"url\" : \"{baseUrl}\"}}"));

            environmentMock.Setup(x => x.IsProcessRunning(100)).Returns(true);

            var clientLookup = new ClientLookup(environmentMock.Object);
            var response = clientLookup.GetSystemClient();


            Assert.Equal(forHaipaZero, response.IsHaipaZero);
            Assert.Equal(identityEndpoint, response.IdentityEndpoint);
            Assert.Equal(apiEndpoint, response.ApiEndpoint);
            Assert.NotNull(response.Client.KeyPair);
            Assert.Equal("system-client", response.Client.ClientName);

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
                .Returns(new StringReader(PrivateKeyFile));

            environmentMock.Setup(x => x.FileSystem).Returns(fileSystemMock.Object);
            return fileSystemMock;
        }



        private const string PrivateKeyFile = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQB5mHPDgNW+Nzr+KaeMeGVrdUnh0sPnilsIA91+Zvtec2KhP06o o4uXETf5oOJLsOvsXPcFjkR+wv12YPevOXAxKqbE7/BPU65FQhMWSGK+F+lNbQMb FKLSsoeaFxSMitf97LAVzSMpt8OmJ732kQ7gSQ7rABRC9/q7R960cNY/5rK2xV4C 7a47f2iolxBDUlhZ2rW8Mdh6kywK29/mAjvt9S0VN5Mvov3wSsD6J28iqvJzdy/5 SNveH7P4K3MBleALBRePY4D00P2le4KdqBxs64ydLg+xqBwOp4aIj6mWGDQlc4TC Y32hHTKK4aPp7sZB3V6oSiK/l06ac6vtquDHAgMBAAECggEAE+xIu3W2j84Y2mAU 1c08QNkc2+Vet+dRdwS7G+TftuAM/wKSbsstKflmRH551ZENdtLcnopq6qIkSWsl 6g3tNgEZBheSNk0ttqdW3UXK9/6O+WKtKZi9/OvHkBXMBiMRtMc9KrVL16AGbIkC dQ3bdCBEU3jV2Qssh9cExGfgkuOMkIgGgAgM/Qk6X+io+JW2hpSmrdra4z6ffPST oCVfbQ+QsLPGmybGs0BzhnzFGcACzskVZHB8rFOqZERXbBdnwjbnmmOK96sgESer BL6VsEUllQXY2N/t4QXqpZP5er16uRAEgjkukqGi2nn6Hjs8UhvteyRfU0JQRPnq x1+eoQKBgQDcWKCBAsl/OxidQtCGPzLMuuua/QEdvlofb+ogr7V0aZ1aJi9OxhBJ 3JjuguRovU7p6rbAgr+V+4LsKL3VOO4sildoXavPY2R9Eeuh/y1oNY+dnw0OkEo3 0uBuIYdek8s66yFGDPmbfu3+tzeF7fu6+G/eRSsX9LBXExBb24OxXwKBgQCNRUm+ b4Lm1Yfya9qju/ciDD0g4rlRc/MtzSaovLBxcTMFIrUguYyOXEkYiOWUbPZEvIk3 ZsGzb+Iv5lhn2JvI0z8Rh5BfPdYrgbAbEPiemKgJQs2pMGpds8vGpDFGCHnzHJko MolJRbOsRER+lMhnbCI5B9zx6W0/i1dNj4uBmQKBgQCWsu6jDVrt72b4NzgSeKqv pq94gsz+oK9WjN4dmM6LXahGfZMhVwjQ21Sk21SH5eFQzjxLEaEiXK/AAGVErPkH 8V2yfU4COsIBX/49/x35BZjBfoQZj8mSwGDKMZg5sO7vztwk4r7cAEWZTYllycu+ picsZzX/3lO0Wc94Y3uAFQKBgG4uPifC/QtgOxl9uRa+wS7S8NI3QmYe0ulD+gTc tZikuzAkM7SEQvW9UF1MWBJ9MU3G5hZJlIWIm5bURtsne8kTyTq4yocdyW5BRcK2 Z9H6KgSfD5wHYM4YLrSM1slSTxqnkWRileSJ8mpHDEzVacAP/FkSouYiMsy+tqaN cDbxAoGBAKtsRWhjxeABPpwdEFFuFyDcoCGb8MIKHiob4AXrNP0OqwecH7DMVvQf mFcM1j8PydHzHFqJXjS0qVhxRPhODBnS1d2lTwvrM7Cqmrgd7QUpZMKNb7rGGSf4 1ruNA/IDbxQURAYjZBL7IGzmvLt+nAyZMd08B9ACyk2D3gtXDIdz 
-----END RSA PRIVATE KEY-----";
    }
}
