using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Eryph.IdentityModel.Clients;

[PublicAPI]
public sealed class ClientCredentials(
    string id,
    SecureString keyPairData,
    Uri identityProvider,
    string configuration)
{
    public string Id { get; } = id;

    public SecureString KeyPairData { get; } = keyPairData;

    public Uri IdentityProvider { get; } = identityProvider;

    public string Configuration { get; } = configuration;

    public Task<AccessTokenResponse> GetAccessToken(HttpClient httpClient = null)
    {
        return GetAccessToken(null, httpClient);
    }

    public async Task<AccessTokenResponse> GetAccessToken(IEnumerable<string> scopes, HttpClient httpClient = null)
    {
        var disposeHttpClient = httpClient == null;
        httpClient ??= new HttpClient();

        httpClient.BaseAddress = IdentityProvider;

        var keyPairPtr = Marshal.SecureStringToGlobalAllocUnicode(KeyPairData);
        try
        {
            var keyPair = Internal.PrivateKey.ReadString(Marshal.PtrToStringUni(keyPairPtr));
            if (!disposeHttpClient)
                return await httpClient.GetClientAccessToken(
                    Id,
                    keyPair.ToRSAParameters(), scopes).ConfigureAwait(false);

            using (httpClient)
            {
                var result = await httpClient.GetClientAccessToken(
                    Id,
                    keyPair.ToRSAParameters(), scopes).ConfigureAwait(false);
                return result;
            }
        }
        finally
        {
            Marshal.ZeroFreeGlobalAllocUnicode(keyPairPtr);
        }
    }
}
