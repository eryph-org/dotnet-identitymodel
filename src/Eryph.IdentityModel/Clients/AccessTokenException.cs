using System;

namespace Eryph.IdentityModel.Clients;

/// <summary>
/// This exception is thrown when an access token cannot be retrieved.
/// </summary>
public class AccessTokenException : Exception
{
    public AccessTokenException(string message)
        : base(message)
    {
    }

    public AccessTokenException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
