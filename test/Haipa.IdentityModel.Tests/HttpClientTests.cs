using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Haipa.IdentityModel.Clients;
using Moq;
using Moq.Protected;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Xunit;

namespace Haipa.IdentityModel.Tests
{
    public class HttpClientTests
    {
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
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{'error':'some error'}"),
                })
                .Verifiable();

            var httpClient = new HttpClient(httpHandlerMock.Object) { BaseAddress = new Uri("http://localhost") };
            
            await Assert.ThrowsAsync<InvalidOperationException>(() =>httpClient.GetClientAccessToken("test-client", GetRSAParameters(), new []{""}));
            
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
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{'access_token':'token','expires_in':'3600', 'scope':'scope1,scope2'}"),
                })
                .Verifiable();

            var httpClient = new HttpClient(httpHandlerMock.Object) {BaseAddress = new Uri("http://localhost/identity")};


            var response = await httpClient.GetClientAccessToken("test-client", GetRSAParameters());
            
            Assert.Equal("token",response.AccessToken);
            Assert.NotNull(response.ExpiresOn);
            Assert.InRange(response.ExpiresOn.Value, 
                DateTimeOffset.UtcNow.AddHours(1).AddSeconds(-2), 
                DateTimeOffset.UtcNow.AddHours(1).AddSeconds(2));
            Assert.Equal(new[] { "scope1", "scope2" }, response.Scopes);

        }

        private RSAParameters GetRSAParameters()
        {
            var keyFile = (AsymmetricCipherKeyPair)new PemReader(new StringReader(KeyFileString)).ReadObject();
            return keyFile.ToRSAParameters();

        }


        private const string KeyFileString = @"-----BEGIN RSA PRIVATE KEY-----
MIIEowIBAAKCAQEA1lVnYA9GCGJDfvjbfaWoeUWltKjDu2ow5X801Lws2c3TDgxL
9hi2n9NU77F2pwLC01hdgWDl50cmUbC8EaMVY3RW71Fi6Zbjfm13g20r3/tRS0R0
JCz98nfrADKRmlFghgWQHZIGDJM/bQuug27TYcvCERBaTtDbsAt/kSAHAFAmWMUK
yQBn06co972UnNm5BOnJh0v8S9d+3Nt4VVYmXCtl2ZytKEG6X2nvOf4I1J+Q4lPa
Fd/7tkYO/RJvCc3jPJFCEdPJFquoIOeL3WAR155ttrljrro468y37F7UhoUz3tW7
xjvxtt84Oxs6cBDaLG9OkohX1Iy7vvJxDhFpLQIDAQABAoIBAA9LyNpIZMNYPesR
GAWjcv6f9jzfzBjd3HpNuye70TNEYYhPtNkYjOn4PfLINl33ZWTfWiGlAxX1FC/s
HhPf0Ljdto7oPu1JjS06jeGwLXDT7EqOlVT37VymlbvDIB8u/mWPfpLkVwWtOYTO
+rKbEBSwjbDu5ZRZNUse8EjwtpyH5PupxzH/QOE+y50UPiuMi2msSsVky7bCm6Qr
X6Af/rs7eiJnFSk3klh5IRXUi+y2qXMIV6gxwXxF1uEhIZp+MlV/Y9RKX4Rp8geh
c97AQdg53m5L8tsBtm+Bzc2OBsD5LEY96usdVWydCRWoqE0hUVX6Or0uuTTqfy3j
UdIWEDECgYEA9ox5vgDNXNXT0MJpNJK7+9yJhzXBaE+WNTS0BIcep2dDryajjAxe
4b4WxNTWgTmXt5emq+XbX+VuL1MyZtqH5k+mpzdfVcFML6CZMESzYPvrM/k17mYW
KUNfDYlaa1sKPTd+bMeuEsUM6dXQ+qT1nIWVGL3NOLArD09b5VvvTTECgYEA3ozI
YIfl0ZHtxWJTNSAAwkJLpTEOmPQ91e+/I4S/KhY2sJkisBxFLJUAcuFQP2jcf7iR
fg2RLlb4xStDcNOZRIDr0+HBZ5vrUlZvTuaBWpCcO5scLI6ez0zl/hfpdbMZbjMS
zuBToXiUyXc5rQF+JhX8+ARoO0qC9NMa/QLxLL0CgYEAk+2xVhlxHpSFpKohKZQp
CnNGaUQNqaKnA4F9yYGxGMxSxhKu6ma5v0SosKzrj1mY+GUbceRWffFQ7UBD64aP
J9b+rTICF5gFOEZp45Y08qn0c5jBjSrffR6ZN6wD/on/WL+lMWuVvFlS6DKMUvcL
D0DvNosbSToae/MntjQ1HuECgYA83bwfyossWgDxrwaazPnoJ0GRGG2pn4MZ88wO
5stxs0mZ2wgFqnWwz7+jq8PK098af1wrYYKHbfnz0vVK8lREzA2zkVbYA2jEyCcB
KUHPhyVzl+SIuyjsAVgVumx7aFRYM1e9hNTaoKPwxc7cZkAeIn1hR7NKJALU+rey
4w8a8QKBgEDj9xGtwFOioJDhE/Di9mOwvCIVrKqc9grWDNQPg0xVoc9yDpxmTckD
A6QX7E8p7pwxVaWP3liLmQbu7Gnm2WrlA8NAlonXoRW9ZKAYeN0Wy4cIkX5W90rA
unFU7rRcjmDSkvR1hN+of2miBZH5bxU5JRVUdNf2FCZUCAH4bFf+
-----END RSA PRIVATE KEY-----
";
    }
}
