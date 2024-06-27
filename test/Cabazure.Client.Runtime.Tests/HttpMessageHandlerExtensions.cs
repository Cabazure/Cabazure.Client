namespace Cabazure.Client.Runtime.Tests;

public static class HttpMessageHandlerExtensions
{
    public static IEnumerable<HttpMessageHandler> GetHandlerChain(
        this HttpMessageHandler handler)
    {
        yield return handler;

        if (handler is DelegatingHandler { InnerHandler: { } innerHandler })
        {
            foreach (var h in GetHandlerChain(innerHandler))
            {
                yield return h;
            }
        }
    }
}
