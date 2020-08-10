using System.Security.Cryptography.X509Certificates;

namespace Haipa.IdentityModel.Clients
{
    public static class X509CertificateExtensions
    {
        public static string CreateClientAuthJwt(this X509Certificate2 issuerCertificate, string audience)
        {
            var cn = issuerCertificate.GetNameInfo(X509NameType.SimpleName, false);
            return ClientAuth.CreateJwt(audience, cn, issuerCertificate);
        }

        public static string CreateClientAuthJwt(this X509Certificate2 issuerCertificate, string audience, string issuerName)
        {
            return ClientAuth.CreateJwt(audience, issuerName, issuerCertificate);
        }
    }
}
