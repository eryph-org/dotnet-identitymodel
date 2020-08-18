using Haipa.IdentityModel.Clients;
using Xunit;

namespace IdentityModel.Clients.Tests
{
    public class ClientGeneratorTests
    {

        [Fact]
        public void NewClient_creates_Client()
        {
            var clientGenerator = new ClientGenerator();
            var result = clientGenerator.NewClient("test-client");

            Assert.Equal("test-client", result.ClientName);
            Assert.NotNull(result.Certificate);
            Assert.NotNull(result.KeyPair);
        }
    }
}