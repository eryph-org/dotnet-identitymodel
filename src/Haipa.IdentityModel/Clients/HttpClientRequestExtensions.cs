using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    internal static class HttpClientExtensions
    {
        public static async Task<AccessTokenResponse> GetClientAccessToken(this HttpClient httpClient, string clientName, X509Certificate2 clientCertificate, IEnumerable<string> scopes = null)
        {

            var jwt = CreateClientAuthJwt("http://localhost/connect/token", clientName, clientCertificate);


            var properties = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientName,
                ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                ["client_assertion"] = jwt

            };

            if(scopes!= null)
                properties.Add("scopes", string.Join(",", scopes.Select(x => x)));

            var request = new HttpRequestMessage(HttpMethod.Post, "connect/token")
            {
                Content = new FormUrlEncodedContent(properties)
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var payload = JObject.Parse(content);
            if (payload["error"] != null)
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }
            payload.TryGetValue("access_token", out var accessToken);
            payload.TryGetValue("expires_in", out var expiresIn);
            payload.TryGetValue("scope", out var scopesResponse);

            var tokenResponse = new AccessTokenResponse
            {
                AccessToken = accessToken?.ToString(),
                ExpiresOn = expiresIn == null ? default : DateTimeOffset.UtcNow.AddSeconds(expiresIn.ToObject<int>()),
                Scopes = scopes?.ToString().Split(',')
            };


            return tokenResponse;
        }

        private static string CreateClientAuthJwt(string audience, string issuerName, X509Certificate2 issuerCert)
        {
            // set exp to 5 minutes
            var tokenHandler = new JwtSecurityTokenHandler { TokenLifetimeInMinutes = 5 };
            var securityToken = tokenHandler.CreateJwtSecurityToken(
                // iss must be the client_id of our application
                issuer: issuerName,
                // aud must be the identity provider (token endpoint)
                audience: audience,
                // sub must be the client_id of our application
                subject: new ClaimsIdentity(
                    new List<Claim> { new Claim("sub", issuerName) }),
                // sign with the private key (using RS256 for IdentityServer)
                signingCredentials: new SigningCredentials(
                    new X509SecurityKey(issuerCert), "RS256")
            );

            return tokenHandler.WriteToken(securityToken);

        }

        public static async Task<string> GetResourceAsync(this HttpClient client, string token, string resource)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, resource);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

    }

    public sealed class AccessTokenResponse
    {
        /// <summary>Gets the Access Token requested.</summary>
        [DataMember]
        public string AccessToken { get; internal set; }

        /// <summary>
        /// Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
        /// This value is calculated based on current UTC time measured locally and the value expiresIn received from the service.
        /// </summary>
        [DataMember]
        public DateTimeOffset? ExpiresOn { get; internal set; }

        /// <summary>
        /// The scopes for this access token
        /// </summary>
        [DataMember]
        public IEnumerable<string> Scopes { get; internal set; }

    }
}