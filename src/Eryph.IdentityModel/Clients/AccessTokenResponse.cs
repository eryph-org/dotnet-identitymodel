using System;
using System.Collections.Generic;

namespace Eryph.IdentityModel.Clients;

/// <summary>
/// Contains the requested access token and some metadata about it.
/// </summary>
public sealed class AccessTokenResponse
{
    /// <summary>
    /// The requested access token.
    /// </summary>
    public string AccessToken { get; internal set; }

    /// <summary>
    /// Gets the point in time when the access token ceases to be valid.
    /// This value is calculated based on current UTC time measured locally and the
    /// value <c>expiresIn</c> received from the service.
    /// </summary>
    public DateTimeOffset? ExpiresOn { get; internal set; }

    /// <summary>
    /// The scopes which have been granted to this access token.
    /// </summary>
    public IEnumerable<string> Scopes { get; internal set; }
}
