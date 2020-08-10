using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace Haipa.IdentityModel.Clients
{
    public static class ClientAuth
    {
        public static string CreateJwt(string audience, string issuerName, X509Certificate2 issuerCert)
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
    }
}