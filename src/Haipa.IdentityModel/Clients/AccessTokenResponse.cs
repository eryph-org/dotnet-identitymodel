using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Haipa.IdentityModel.Clients
{
    public sealed class AccessTokenResponse
    {
        /// <summary>Gets the Access Token requested.</summary>
        [DataMember]
        public string AccessToken { get; internal set; }

        /// <summary>
        ///     Gets the point in time in which the Access Token returned in the AccessToken property ceases to be valid.
        ///     This value is calculated based on current UTC time measured locally and the value expiresIn received from the
        ///     service.
        /// </summary>
        [DataMember]
        public DateTimeOffset? ExpiresOn { get; internal set; }

        /// <summary>
        ///     The scopes for this access token
        /// </summary>
        [DataMember]
        public IEnumerable<string> Scopes { get; internal set; }
    }
}