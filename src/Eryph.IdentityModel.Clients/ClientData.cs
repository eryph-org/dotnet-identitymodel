using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;

namespace Haipa.IdentityModel.Clients
{
    [PublicAPI]
    public sealed class ClientData
    {

        public ClientData(string id, string name)
        {
            Id = id;
            Name = name;
        }

        /// <summary>
        /// constructor for deserialization
        /// </summary>
        [ExcludeFromCodeCoverage]
        internal ClientData()
        {}


        [DataMember]
        public string Id { get; }


        [DataMember]
        public string Name { get; }

    }
}