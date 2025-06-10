namespace Cabazure.Client;

public static class HttpRequestMessageExtensions
{
    public static void SetScopes(
        this HttpRequestMessage request,
        params string[] scopes)
        => request.Properties["scopes"] = scopes;

    public static string[]? GetScopes(this HttpRequestMessage request)
    {
        if (request.Properties.TryGetValue("scopes", out var value) && value is string[] scopes)
        {
            return scopes;
        }
        return null;
    }
}