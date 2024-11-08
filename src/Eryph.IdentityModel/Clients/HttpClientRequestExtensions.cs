using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Eryph.IdentityModel.Clients;

public static class HttpClientExtensions
{
    private static readonly Lazy<JsonSerializerOptions> JsonOptions = new(() =>
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
        });

    public static async Task<AccessTokenResponse> GetClientAccessToken(
        this HttpClient httpClient,
        Uri tokenEndpointUrl,
        string clientName,
        RSAParameters rsaParameters,
        IReadOnlyList<string> scopes = null)
    {
        if (!tokenEndpointUrl.IsAbsoluteUri)
            throw new AccessTokenException("The token endpoint URL is not an absolute URL.");

        var audience = tokenEndpointUrl.AbsoluteUri;
        var jwt = CreateClientAssertionJwt(audience, clientName, rsaParameters);

        var properties = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = clientName,
            ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
            ["client_assertion"] = jwt
        };
        
        if (scopes is { Count: > 0 })
            properties.Add("scopes", string.Join(",", scopes));

        var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpointUrl)
        {
            Content = new FormUrlEncodedContent(properties)
        };

        try
        {
            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest)
                throw CreateException(response.StatusCode);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                try
                {
                    var error = JsonSerializer.Deserialize<TokenErrorJsonResponse>(content, JsonOptions.Value);
                    if (error is null)
                        throw CreateException(response.StatusCode);

                    var message = $"Could not retrieve an access token: {error.Error}."
                        + (string.IsNullOrEmpty(error.ErrorDescription) ? "" : $" {error.ErrorDescription}");

                    throw new AccessTokenException(message);
                }
                catch (Exception ex) when (ex is not AccessTokenException)
                {
                    throw CreateException(response.StatusCode);
                }
            }

            var tokenJsonResponse = JsonSerializer.Deserialize<TokenJsonResponse>(content, JsonOptions.Value);
            if (string.IsNullOrEmpty(tokenJsonResponse.AccessToken))
                throw new AccessTokenException("Could not retrieve an access token. The access token is missing in the response.");

            var tokenResponse = new AccessTokenResponse
            {
                AccessToken = tokenJsonResponse.AccessToken,
                ExpiresOn = tokenJsonResponse.ExpiresIn.HasValue
                    ? DateTimeOffset.UtcNow.AddSeconds(tokenJsonResponse.ExpiresIn.Value)
                    : null,
                Scopes = tokenJsonResponse.Scope?.Split(','),
            };

            return tokenResponse;
        }
        catch (Exception ex) when (ex is not AccessTokenException)
        {
            throw new AccessTokenException("Could not retrieve an access token.", ex);
        }
    }

    private static AccessTokenException CreateException(
        HttpStatusCode statusCode) =>
        new($"Could not retrieve an access token. The server responded with {statusCode}.");

    private static string CreateClientAssertionJwt(string audience, string clientName, RSAParameters rsaParameters)
    {
        // Set exp to 5 minutes
        var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 5 };
        var securityToken = tokenHandler.CreateJwtSecurityToken(
            // iss must be the client_id of our application
            clientName,
            // aud must be the identity provider (token endpoint)
            audience,
            // sub must be the client_id of our application
            subject: new ClaimsIdentity(
                [
                    new Claim("sub", clientName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                ]),
            // Sign with the private key (using RS256 for IdentityServer)
            signingCredentials: new SigningCredentials(new RsaSecurityKey(rsaParameters), "RS256")
        );

        return tokenHandler.WriteToken(securityToken);
    }

    private sealed class TokenJsonResponse
    {
        [JsonRequired] public string AccessToken { get; set; }

        public int? ExpiresIn { get; set; }
        
        public string Scope { get; set; }
    }

    private sealed class TokenErrorJsonResponse
    {
        [JsonRequired] public string Error { get; set; }

        public string ErrorDescription { get; set; }
    }
}
