namespace Cabazure.Client.SourceGenerator;

internal static class HttpHeaderNameExtensions
{
    private static readonly HashSet<string> ContentHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Allow",
        "Content-Disposition",
        "Content-Encoding",
        "Content-Language",
        "Content-Length",
        "Content-Location",
        "Content-MD5",
        "Content-Range",
        "Content-Type",
        "Expires",
        "Last-Modified",
    };

    internal static bool IsContentHeaderName(this string name)
        => ContentHeaderNames.Contains(name);
}
