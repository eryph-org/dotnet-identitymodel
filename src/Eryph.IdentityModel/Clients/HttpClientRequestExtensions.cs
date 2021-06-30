using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

namespace Haipa.IdentityModel.Clients
{
    public static class HttpClientExtensions
    {
        public static async Task<AccessTokenResponse> GetClientAccessToken(this HttpClient httpClient,
            string clientName, RSAParameters rsaParameters, IEnumerable<string> scopes = null)
        {
            var fullAddress = httpClient.BaseAddress;
            if (fullAddress.PathAndQuery != "" && !fullAddress.PathAndQuery.EndsWith("/"))
                fullAddress = new Uri(fullAddress + "/");

            var audience = fullAddress + "connect/token";
            var jwt = CreateClientAssertionJwt(audience, clientName, rsaParameters);


            var properties = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = clientName,
                ["client_assertion_type"] = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer",
                ["client_assertion"] = jwt
            };

            var scopesArray = (scopes ?? Array.Empty<string>()).ToArray();
            if (scopesArray.Any())
                properties.Add("scopes", string.Join(",", scopesArray.Select(x => x)));

            var request = new HttpRequestMessage(HttpMethod.Post, audience)
            {
                Content = new FormUrlEncodedContent(properties)
            };

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead)
                .ConfigureAwait(false);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            try
            {
                var payload = JObject.Parse(content);
                if (payload["error"] != null)
                    throw new InvalidOperationException();

                payload.TryGetValue("access_token", out var accessToken);
                payload.TryGetValue("expires_in", out var expiresIn);
                payload.TryGetValue("scope", out var scopesResponse);

                var tokenResponse = new AccessTokenResponse
                {
                    AccessToken = accessToken?.ToString(),
                    ExpiresOn = expiresIn == null
                        ? default
                        : DateTimeOffset.UtcNow.AddSeconds(expiresIn.ToObject<int>()),
                    Scopes = scopesResponse?.ToString().Split(',')
                };


                return tokenResponse;
            }
            catch
            {
                throw new InvalidOperationException("An error occurred while retrieving an access token.");
            }
        }

        private static string CreateClientAssertionJwt(string audience, string clientName, RSAParameters rsaParameters)
        {
            // set exp to 5 minutes
            var tokenHandler = new JwtSecurityTokenHandler {TokenLifetimeInMinutes = 5};
            var securityToken = tokenHandler.CreateJwtSecurityToken(
                
                // iss must be the client_id of our application
                clientName,
                // aud must be the identity provider (token endpoint)
                audience,
                // sub must be the client_id of our application
                subject: new ClaimsIdentity(
                    new List<Claim>
                    {
                        new Claim("sub", clientName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    }),
                // sign with the private key (using RS256 for IdentityServer)
                signingCredentials: new SigningCredentials(
                    new RsaSecurityKey(rsaParameters), "RS256")
            );

            return tokenHandler.WriteToken(securityToken);
        }
    }
}