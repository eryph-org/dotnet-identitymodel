using System;
using System.Collections.Generic;
using System.Text;
using Haipa.IdentityModel.Clients.Cryptography;

namespace Haipa.IdentityModel.Clients
{
    public class ClientGenerator
    {
        public GeneratedClientData NewClient(string id)
        {
            var (certificate, keyPair) = X509Generation.GenerateCertificate(id);
           return new GeneratedClientData(id, certificate, keyPair);
        }
    }



}
