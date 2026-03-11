namespace Cabazure.Client;

public static class HttpRequestMessageExtensions
{
    private const string ScopesKey = "scopes";

    public static void SetScopes(
        this HttpRequestMessage request,
        params string[] scopes)
    {
#if NET5_0_OR_GREATER
        request.Options.Set(new HttpRequestOptionsKey<string[]>(ScopesKey), scopes);
#else
        request.Properties[ScopesKey] = scopes;
#endif
    }

    public static string[]? GetScopes(this HttpRequestMessage request)
    {
#if NET5_0_OR_GREATER
        if (request.Options.TryGetValue(new HttpRequestOptionsKey<string[]>(ScopesKey), out var scopes))
        {
            return scopes;
        }
        return null;
#else
        if (request.Properties.TryGetValue(ScopesKey, out var value) && value is string[] scopes)
        {
            return scopes;
        }
        return null;
#endif
    }
}