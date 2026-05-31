using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Eryph.IdentityModel.Clients;
using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using Moq.Protected;
using Xunit;

namespace Eryph.IdentityModel.Tests;

public class HttpClientExtensionsTests
{
    private const string Issuer = "https://identity.test/identity";
    private const string TokenEndpoint = "https://identity.test/identity/connect/token";

    private readonly Uri _tokenUrl = new(TokenEndpoint);
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
            _rsaParameters,
            ["test-scope"]);

        response.AccessToken.Should().Be("token");
        response.ExpiresOn.Should().BeNull();
        response.Scopes.Should().Equal("test-scope");
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

    [Fact]
    public async Task GetClientAccessToken_WithIssuerAudienceAndTokenType_SetsThemOnAssertion()
    {
        var assertion = await CaptureAssertion(
            audience: Issuer, tokenType: "client-authentication+jwt");

        assertion.Audiences.Should().ContainSingle().Which.Should().Be(Issuer);
        assertion.Header["typ"].Should().Be("client-authentication+jwt");
        assertion.Issuer.Should().Be("test-client");
        assertion.Subject.Should().Be("test-client");
    }

    [Fact]
    public async Task GetClientAccessToken_WithoutAudience_UsesTokenEndpointAndPlainJwtType()
    {
        var assertion = await CaptureAssertion(audience: null, tokenType: null);

        assertion.Audiences.Should().ContainSingle().Which.Should().Be(TokenEndpoint);
        assertion.Header["typ"].Should().Be("JWT");
    }

    private async Task<JwtSecurityToken> CaptureAssertion(string audience, string tokenType)
    {
        string capturedAssertion = null;
        _messageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) =>
                capturedAssertion = HttpUtility.ParseQueryString(
                    req.Content.ReadAsStringAsync().GetAwaiter().GetResult())["client_assertion"])
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("""{ "access_token": "token" }"""),
            });

        using var httpClient = new HttpClient(_messageHandlerMock.Object);
        await httpClient.GetClientAccessToken(
            _tokenUrl, "test-client", _rsaParameters, scopes: null, audience: audience, tokenType: tokenType);

        return new JwtSecurityTokenHandler().ReadJwtToken(capturedAssertion);
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