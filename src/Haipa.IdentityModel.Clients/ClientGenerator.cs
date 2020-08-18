using System;
using System.Collections.Generic;
using System.Text;
using Haipa.IdentityModel.Clients.Internal;

namespace Haipa.IdentityModel.Clients
{
    public class ClientGenerator
    {
        public GeneratedClientData NewClient(string clientName)
        {
            var (certificate, keyPair) = X509Generation.GenerateSelfSignedCertificate(clientName);
           return new GeneratedClientData(clientName, certificate, keyPair);
        }
    }



}
