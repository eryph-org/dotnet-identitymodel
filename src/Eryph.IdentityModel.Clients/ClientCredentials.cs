using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Eryph.IdentityModel.Clients;

[PublicAPI]
public sealed class ClientCredentials
{
    // Custom OpenID Connect discovery metadata entry advertised by eryph servers running
    // OpenIddict 7.0 and higher. When present (value "issuer") the server expects the issuer
    // as the client-assertion audience and the "client-authentication+jwt" token type.
    private const string ClientAssertionAudienceMetadata = "eryph_client_assertion_audience";
    private const string ClientAssertionAudienceIssuer = "issuer";
    private const string ClientAuthenticationJwtType = "client-authentication+jwt";

    private readonly Uri _tokenUrl;
    private readonly Uri _metadataUrl;

    public ClientCredentials(string id,
        SecureString keyPairData,
        Uri identityProvider,
        string configuration)
    {
        Id = id;
        KeyPairData = keyPairData;
        IdentityProvider = identityProvider;
        Configuration = configuration;
        var uriBuilder = new UriBuilder(IdentityProvider);
        uriBuilder.Path += uriBuilder.Path.EndsWith("/") ? "connect/token" : "/connect/token";
        _tokenUrl = uriBuilder.Uri;

        var metadataBuilder = new UriBuilder(IdentityProvider);
        metadataBuilder.Path += metadataBuilder.Path.EndsWith("/")
            ? ".well-known/openid-configuration"
            : "/.well-known/openid-configuration";
        _metadataUrl = metadataBuilder.Uri;
    }

    public string Id { get; }

    public SecureString KeyPairData { get; }

    public Uri IdentityProvider { get; }

    public string Configuration { get; }

    public Task<AccessTokenResponse> GetAccessToken(
        HttpClient httpClient = null)
    {
        return GetAccessToken(null, httpClient);
    }

    public async Task<AccessTokenResponse> GetAccessToken(
        IReadOnlyList<string> scopes,
        HttpClient httpClient = null)
    {
        var disposeHttpClient = httpClient == null;
        httpClient ??= new HttpClient();

        try
        {
            // Resolve the assertion format (a network call to the discovery endpoint) before pinning
            // the private key in unmanaged memory, and unpin it immediately after reading it, so the
            // plaintext key is exposed for as short a time as possible.
            var (audience, tokenType) = await ResolveAssertionFormat(httpClient).ConfigureAwait(false);
            var rsaParameters = ReadRsaParameters();

            return await httpClient.GetClientAccessToken(
                    _tokenUrl, Id, rsaParameters, scopes, audience, tokenType)
                .ConfigureAwait(false);
        }
        finally
        {
            if (disposeHttpClient)
                httpClient.Dispose();
        }
    }

    private RSAParameters ReadRsaParameters()
    {
        var keyPairPtr = Marshal.SecureStringToGlobalAllocUnicode(KeyPairData);
        try
        {
            return Internal.PrivateKey.ReadString(Marshal.PtrToStringUni(keyPairPtr)).ToRSAParameters();
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(keyPairPtr);
        }
    }

    /// <summary>
    /// Detects the expected client-assertion format from the server's OpenID Connect discovery
    /// document. Newer eryph servers (OpenIddict 7.0+) advertise <c>eryph_client_assertion_audience
    /// = issuer</c>, which requires the issuer as the audience and the <c>client-authentication+jwt</c>
    /// token type; older servers expect the token endpoint as the audience.
    /// </summary>
    /// <remarks>
    /// Every eryph identity server serves the discovery document, so a failure to read it is a real
    /// error and is surfaced rather than silently downgrading to a format the server would reject.
    /// </remarks>
    private async Task<(string Audience, string TokenType)> ResolveAssertionFormat(HttpClient httpClient)
    {
        // Read the discovery document directly and parse it without fetching the server's signing
        // keys (jwks_uri): the client signs assertions with its own key.
        OpenIdConnectConfiguration configuration;
        try
        {
            var json = await httpClient.GetStringAsync(_metadataUrl).ConfigureAwait(false);
            configuration = OpenIdConnectConfiguration.Create(json);
        }
        catch (Exception ex)
        {
            throw new AccessTokenException(
                "Could not read the identity provider's discovery document.", ex);
        }

        var advertisesIssuerAudience =
            configuration.AdditionalData.TryGetValue(ClientAssertionAudienceMetadata, out var value)
            && string.Equals(value?.ToString(), ClientAssertionAudienceIssuer, StringComparison.OrdinalIgnoreCase);

        if (advertisesIssuerAudience)
        {
            // The server explicitly requires the issuer as the audience, so a discovery document
            // that advertises this but omits the issuer is invalid and must fail rather than
            // downgrade to a legacy assertion the server would reject.
            if (string.IsNullOrEmpty(configuration.Issuer))
                throw new AccessTokenException(
                    "The identity provider requires the issuer as the client-assertion audience, " +
                    "but its discovery document does not contain an issuer.");

            return (configuration.Issuer, ClientAuthenticationJwtType);
        }

        // Older servers don't advertise the flag and expect the token endpoint as the audience.
        return (_tokenUrl.AbsoluteUri, null);
    }
}
