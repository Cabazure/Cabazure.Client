using Azure.Core;
using Cabazure.Client.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder SetBaseAddress(
            this IHttpClientBuilder builder,
            Uri baseAddress)
            => builder.ConfigureHttpClient(c => c.BaseAddress = baseAddress);

        public static IHttpClientBuilder AddAuthentication(
            this IHttpClientBuilder builder,
            string scope,
            TokenCredential credential)
            => AddAuthentication(
                builder,
                new[] { scope },
                credential);

        public static IHttpClientBuilder AddAuthentication(
            this IHttpClientBuilder builder,
            string[] scopes,
            TokenCredential credential)
        {
            var tokenProvider = new BearerTokenProvider(
                credential,
                new DateTimeProvider());

            return builder
                .AddHttpMessageHandler(
                    () => new AzureAuthenticationHandler(scopes, tokenProvider));
        }
    }
}
