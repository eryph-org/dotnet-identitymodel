using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Eryph.IdentityModel.Clients;

[PublicAPI]
public sealed class ClientCredentials
{
    private readonly Uri _tokenUrl;

    public ClientCredentials(string id,
        SecureString keyPairData,
        Uri identityProvider,
        string configuration)
    {
        Id = id;
        KeyPairData = keyPairData;
        IdentityProvider = identityProvider;
        Configuration = configuration;
        var uriBuilder = new UriBuilder(IdentityProvider);
        uriBuilder.Path += uriBuilder.Path.EndsWith("/") ? "connect/token" : "/connect/token";
        _tokenUrl = uriBuilder.Uri;
    }

    public string Id { get; }

    public SecureString KeyPairData { get; }

    public Uri IdentityProvider { get; }

    public string Configuration { get; }

    public Task<AccessTokenResponse> GetAccessToken(
        HttpClient httpClient = null)
    {
        return GetAccessToken(null, httpClient);
    }

    public async Task<AccessTokenResponse> GetAccessToken(
        IReadOnlyList<string> scopes,
        HttpClient httpClient = null)
    {
        var disposeHttpClient = httpClient == null;
        var keyPairPtr = Marshal.SecureStringToGlobalAllocUnicode(KeyPairData);
        
        try
        {
            httpClient ??= new HttpClient();
            var keyPair = Internal.PrivateKey.ReadString(Marshal.PtrToStringUni(keyPairPtr));
            return await httpClient.GetClientAccessToken(
                    _tokenUrl, Id, keyPair.ToRSAParameters(), scopes)
                .ConfigureAwait(false);
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(keyPairPtr);
            if (disposeHttpClient)
                httpClient?.Dispose();
            
        }
    }
}
