using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Azure.Core;

namespace Cabazure.Client.Authentication
{
    public class BearerTokenProvider : IBearerTokenProvider
    {
        private readonly TokenCredential credential;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ConcurrentDictionary<string[], AccessToken> accessTokenCache = new();

        public BearerTokenProvider(
            TokenCredential credential,
            IDateTimeProvider dateTimeProvider)
        {
            this.credential = credential;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<AuthenticationHeaderValue> GetTokenAsync(
            string[] scopes,
            CancellationToken cancellationToken)
        {
            if (!accessTokenCache.TryGetValue(scopes, out var accessToken) || TokenIsExpired(accessToken))
            {
                accessTokenCache[scopes] = accessToken = await credential.GetTokenAsync(new TokenRequestContext(scopes), cancellationToken);
            }

            return new AuthenticationHeaderValue(
                "Bearer",
                accessToken.Token);
        }

        private bool TokenIsExpired(AccessToken accessToken)
            => dateTimeProvider.GetDateTime().AddMinutes(1) > accessToken.ExpiresOn;
    }
}
