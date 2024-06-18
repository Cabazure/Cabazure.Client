using System.Net.Http.Headers;

namespace Cabazure.Client.Authentication;

public interface IBearerTokenProvider
{
    Task<AuthenticationHeaderValue> GetTokenAsync(
        CancellationToken cancellationToken);
}
