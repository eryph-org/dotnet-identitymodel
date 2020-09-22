using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Haipa.IdentityModel.Clients;
using Moq;
using Xunit;

namespace IdentityModel.Clients.Tests
{
    public class ConfigStoreTests
    {

        [Fact]
        public void GetClients_reads_only_valid_clients()
        {
            var filesystemMock = new Mock<IFileSystem>();
            var environmentMock = new Mock<IEnvironment>();
            environmentMock.Setup(x => x.FileSystem).Returns(filesystemMock.Object);

            filesystemMock.Setup(x => x.FileExists(It.Is<string>(x => x.EndsWith("default.config")))).Returns(true);
            filesystemMock.Setup(x => x.OpenText(It.Is<string>(x => x.EndsWith("default.config"))))
                .Returns(new StringReader(
                "{ \"clients\" : [ {\"id\" : \"id-1\" }, {\"id\" : \"id-2\" }, {\"id\" : \"id-3\" }  ]}"));


            filesystemMock.Setup(x => x.FileExists(It.Is<string>(x => 
                x.EndsWith("id-1.key") || x.EndsWith("id-3.key")))).Returns(true);

            filesystemMock.Setup(x => x.OpenText(It.Is<string>(
                x => x.EndsWith(".key")
            ))).Returns(() =>  new StringReader(TestData.PrivateKeyFileString));


            var configStore = ConfigStore.GetStore(ConfigStoreLocation.CurrentDirectory, environmentMock.Object);
            var clientIds = configStore.GetClients().Select(x=>x.Id).ToArray();

            Assert.Contains("id-1", clientIds);
            Assert.Contains("id-3", clientIds);

        }

        [Fact]
        public void Creates_new_ConfigStore_with_Endpoint()
        {
            var filesystemMock = new Mock<IFileSystem>();
            var environmentMock = new Mock<IEnvironment>();
            environmentMock.Setup(x => x.FileSystem).Returns(filesystemMock.Object);
            var sb = new StringBuilder();
            filesystemMock.Setup(x => x.CreateText(It.Is<string>(
                p=>p.EndsWith("local.config")))).Returns(() => new StringWriter(sb));

            var writer = new ConfigStoresWriter(environmentMock.Object, "local");
            writer.AddEndpoint("endpointName", new Uri("http://localhost"));
            
            environmentMock.Verify();
            Assert.Equal("{\r\n  \"endpoints\": {\r\n    \"identity\": \"http://localhost\"\r\n  }\r\n}",
                sb.ToString());
        }
    }
}
