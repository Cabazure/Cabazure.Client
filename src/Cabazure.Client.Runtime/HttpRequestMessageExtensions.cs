namespace Cabazure.Client;

public static class HttpRequestMessageExtensions
{
    private const string ScopesKey = "scopes";

    public static void SetScopes(
        this HttpRequestMessage request,
        params string[] scopes)
        => request.Properties[ScopesKey] = scopes;

    public static string[]? GetScopes(this HttpRequestMessage request)
    {
        if (request.Properties.TryGetValue(ScopesKey, out var value) && value is string[] scopes)
        {
            return scopes;
        }

        return null;
    }
}