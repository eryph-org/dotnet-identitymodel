using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Eryph.IdentityModel.Clients;
using FluentAssertions;
using Xunit;

namespace Eryph.IdentityModel.Clients.Tests;

public class ClientCredentialsTests
{
    private const string IdentityProvider = "https://identity.test/identity";
    private const string Issuer = "https://identity.test/identity";
    private const string TokenEndpoint = "https://identity.test/identity/connect/token";

    private static SecureString ToSecureString(string value)
    {
        var secure = new SecureString();
        foreach (var c in value)
            secure.AppendChar(c);
        secure.MakeReadOnly();
        return secure;
    }

    private static ClientCredentials CreateCredentials() =>
        new("test-client", ToSecureString(TestData.PrivateKeyFileString), new Uri(IdentityProvider), "default");

    [Fact]
    public async Task GetAccessToken_NewServer_UsesIssuerAudienceAndJwtType()
    {
        var handler = new FakeIdentityHandler(advertiseIssuerAudience: true);
        using var httpClient = new HttpClient(handler);

        var token = await CreateCredentials().GetAccessToken(["compute_api"], httpClient);

        token.AccessToken.Should().Be("token");

        var assertion = new JwtSecurityTokenHandler().ReadJwtToken(handler.ClientAssertion);
        assertion.Audiences.Should().ContainSingle().Which.Should().Be(Issuer);
        assertion.Header["typ"].Should().Be("client-authentication+jwt");
        assertion.Issuer.Should().Be("test-client");
        assertion.Subject.Should().Be("test-client");
    }

    [Fact]
    public async Task GetAccessToken_LegacyServer_UsesTokenEndpointAudience()
    {
        var handler = new FakeIdentityHandler(advertiseIssuerAudience: false);
        using var httpClient = new HttpClient(handler);

        var token = await CreateCredentials().GetAccessToken(httpClient);

        token.AccessToken.Should().Be("token");

        var assertion = new JwtSecurityTokenHandler().ReadJwtToken(handler.ClientAssertion);
        assertion.Audiences.Should().ContainSingle().Which.Should().Be(TokenEndpoint);
        assertion.Header["typ"].Should().Be("JWT");
    }

    [Fact]
    public async Task GetAccessToken_IssuerAudienceAdvertisedButIssuerMissing_ThrowsAndDoesNotRequestToken()
    {
        // The server requires the issuer as the audience but the discovery document omits it:
        // invalid metadata must fail rather than downgrade to a legacy assertion.
        var handler = new FakeIdentityHandler(advertiseIssuerAudience: true) { IncludeIssuer = false };
        using var httpClient = new HttpClient(handler);

        var act = async () => await CreateCredentials().GetAccessToken(httpClient);

        await act.Should().ThrowAsync<AccessTokenException>();
        handler.TokenRequested.Should().BeFalse();
    }

    [Fact]
    public async Task GetAccessToken_DiscoveryFailure_ThrowsAndDoesNotRequestToken()
    {
        var handler = new FakeIdentityHandler(advertiseIssuerAudience: true)
        {
            DiscoveryStatus = HttpStatusCode.InternalServerError,
        };
        using var httpClient = new HttpClient(handler);

        var act = async () => await CreateCredentials().GetAccessToken(httpClient);

        await act.Should().ThrowAsync<AccessTokenException>();
        handler.TokenRequested.Should().BeFalse();
    }

    private sealed class FakeIdentityHandler : HttpMessageHandler
    {
        private readonly bool _advertiseIssuerAudience;

        public FakeIdentityHandler(bool advertiseIssuerAudience)
        {
            _advertiseIssuerAudience = advertiseIssuerAudience;
        }

        public HttpStatusCode DiscoveryStatus { get; set; } = HttpStatusCode.OK;

        public bool IncludeIssuer { get; set; } = true;

        public bool TokenRequested { get; private set; }

        public string ClientAssertion { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri.AbsolutePath.EndsWith("/.well-known/openid-configuration"))
            {
                if (DiscoveryStatus != HttpStatusCode.OK)
                    return new HttpResponseMessage(DiscoveryStatus);

                var flag = _advertiseIssuerAudience
                    ? ", \"eryph_client_assertion_audience\": \"issuer\""
                    : "";

                var issuerPart = IncludeIssuer ? $"\"issuer\": \"{Issuer}\", " : "";
                var metadata =
                    "{" +
                    issuerPart +
                    $"\"token_endpoint\": \"{TokenEndpoint}\"" +
                    flag +
                    "}";

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(metadata) };
            }

            TokenRequested = true;
            var body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            ClientAssertion = HttpUtility.ParseQueryString(body)["client_assertion"];

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"access_token\":\"token\"}"),
            };
        }
    }
}
