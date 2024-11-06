using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eryph.IdentityModel.Clients;
using Moq;
using Moq.Protected;
using Xunit;

namespace Eryph.IdentityModel.Tests
{
    public class HttpClientTests
    {
        private RSAParameters GetRSAParameters()
        {
            using var rsa = RSA.Create(3072);
            return rsa.ExportParameters(true);
        }

        [Fact]
        public async Task HttpClient_returns_Result()
        {
            var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);


            httpHandlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        {
                          "access_token": "token",
                          "expires_in": 3600,
                          "scope": "scope1,scope2"
                        }
                        """),
                })
                .Verifiable();

            var httpClient = new HttpClient(httpHandlerMock.Object)
                {BaseAddress = new Uri("http://localhost/identity")};


            var response = await httpClient.GetClientAccessToken("test-client", GetRSAParameters());

            Assert.Equal("token", response.AccessToken);
            Assert.NotNull(response.ExpiresOn);
            Assert.InRange(response.ExpiresOn.Value,
                DateTimeOffset.UtcNow.AddHours(1).AddSeconds(-2),
                DateTimeOffset.UtcNow.AddHours(1).AddSeconds(2));
            Assert.Equal(["scope1", "scope2"], response.Scopes);
        }

        [Fact]
        public async Task HttpClient_throws_on_error_response()
        {
            var httpHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            httpHandlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{'error':'some error'}")
                })
                .Verifiable();

            var httpClient = new HttpClient(httpHandlerMock.Object) {BaseAddress = new Uri("http://localhost")};

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                httpClient.GetClientAccessToken("test-client", GetRSAParameters(), new[] {""}));
        }
    }
}
