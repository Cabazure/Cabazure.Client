using System.Net.Http.Headers;
using Azure.Core;

namespace Cabazure.Client.Authentication;

public class BearerTokenProvider(
    TokenRequestContext context,
    TokenCredential credential,
    IDateTimeProvider dateTimeProvider)
    : IBearerTokenProvider
{
    private AccessToken accessToken;

    public async Task<AuthenticationHeaderValue> GetTokenAsync(
        CancellationToken cancellationToken)
    {
        if (TokenIsExpired())
        {
            accessToken = await credential.GetTokenAsync(context, cancellationToken);
        }

        return new AuthenticationHeaderValue(
            "Bearer",
            accessToken.Token);
    }

    private bool TokenIsExpired()
        => dateTimeProvider.GetDateTime().AddMinutes(1) > accessToken.ExpiresOn;
}
