using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Threading;
using Azure.Core;

namespace Cabazure.Client.Authentication
{
    public class BearerTokenProvider : IBearerTokenProvider, IDisposable
    {
        private readonly TokenCredential credential;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly ConcurrentDictionary<string, AccessToken> accessTokenCache = new();
        private readonly SemaphoreSlim semaphore = new(1, 1);
        private bool disposed;

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
            var key = string.Join(" ", scopes);
            
            // Fast path: check cache without lock
            if (accessTokenCache.TryGetValue(key, out var accessToken) && !TokenIsExpired(accessToken))
            {
                return new AuthenticationHeaderValue("Bearer", accessToken.Token);
            }

            // Slow path: acquire lock and refresh token
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                // Double-check: another thread may have refreshed while we waited
                if (accessTokenCache.TryGetValue(key, out accessToken) && !TokenIsExpired(accessToken))
                {
                    return new AuthenticationHeaderValue("Bearer", accessToken.Token);
                }

                // Refresh the token
                accessTokenCache[key] = accessToken = await credential
                    .GetTokenAsync(new TokenRequestContext(scopes), cancellationToken)
                    .ConfigureAwait(false);

                return new AuthenticationHeaderValue("Bearer", accessToken.Token);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private bool TokenIsExpired(AccessToken accessToken)
            => dateTimeProvider.GetDateTime() >= accessToken.ExpiresOn.AddMinutes(-1);

        public void Dispose()
        {
            if (!disposed)
            {
                semaphore.Dispose();
                disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
