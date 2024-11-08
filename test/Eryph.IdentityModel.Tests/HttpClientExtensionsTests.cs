using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Eryph.IdentityModel.Clients;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Eryph.IdentityModel.Tests;

public class HttpClientExtensionsTests
{
    private readonly Uri _tokenUrl = new("https://identity.test/identity/connect/token");
    private readonly Mock<HttpMessageHandler> _messageHandlerMock = new();
    private readonly RSAParameters _rsaParameters;

    public HttpClientExtensionsTests()
    {
        using var rsa = RSA.Create(3072);
        _rsaParameters =  rsa.ExportParameters(true);
    }

    [Fact]
    public async Task GetClientAccessToken_ValidTokenResponse_ReturnsToken()
    {
        ArrangeResponse(
            HttpStatusCode.OK,
            """
            {
                "access_token": "token",
                "expires_in": 3600,
                "scope": "scope1,scope2"
            }
            """);

        var httpClient = new HttpClient(_messageHandlerMock.Object);

        var response = await httpClient.GetClientAccessToken(
            _tokenUrl,
            "test-client",
            _rsaParameters);

        response.AccessToken.Should().Be("token");
        response.ExpiresOn.Should().BeCloseTo(DateTimeOffset.UtcNow.AddHours(1), 2.Seconds());
        response.Scopes.Should().Equal("scope1", "scope2");
    }

    [Fact]
    public async Task GetClientAccessToken_MinimalValidTokenResponse_ReturnsToken()
    {
        ArrangeResponse(
            HttpStatusCode.OK,
            """
            {
                "access_token": "token"
            }
            """);

        var httpClient = new HttpClient(_messageHandlerMock.Object);

        var response = await httpClient.GetClientAccessToken(
            _tokenUrl,
            "test-client",
            _rsaParameters);

        response.AccessToken.Should().Be("token");
        response.ExpiresOn.Should().BeNull();
        response.Scopes.Should().BeNull();
    }

    [Fact]
    public async Task GetClientAccessToken_InvalidTokenResponse_ThrowsException()
    {
        ArrangeResponse(
            HttpStatusCode.BadRequest,
            """
            {
                "error": "test_error",
                "error_description": "test_error_description"
            }
            """);

        var act = async () =>
        {
            using var httpClient = new HttpClient(_messageHandlerMock.Object);
            return await httpClient.GetClientAccessToken(_tokenUrl, "test-client", _rsaParameters);
        };

        await act.Should().ThrowAsync<AccessTokenException>()
            .WithMessage("Could not retrieve an access token: test_error. test_error_description");
    }

    [Fact]
    public async Task GetClientAccessToken_InternalServerError_ThrowsException()
    {
        ArrangeResponse(HttpStatusCode.InternalServerError, "");

        var act = async () =>
        {
            using var httpClient = new HttpClient(_messageHandlerMock.Object);
            return await httpClient.GetClientAccessToken(_tokenUrl, "test-client", _rsaParameters);
        };

        await act.Should().ThrowAsync<AccessTokenException>()
            .WithMessage("Could not retrieve an access token. The server responded with InternalServerError.");
    }

    private void ArrangeResponse(HttpStatusCode statusCode, string response)
    {
        _messageHandlerMock.Protected()
            // Set up the PROTECTED method to mock
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(response)
            });
    }
}