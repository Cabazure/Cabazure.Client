namespace Cabazure.Client.Authentication
{
    public class AzureAuthenticationHandler
        : DelegatingHandler
    {
        private readonly string[] defaultScopes;
        private readonly IBearerTokenProvider tokenProvider;

        public AzureAuthenticationHandler(
            string[] defaultScopes,
            IBearerTokenProvider tokenProvider)
        {
            this.defaultScopes = defaultScopes;
            this.tokenProvider = tokenProvider;
        }

        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var scopes = request.GetScopes() ?? defaultScopes;

            request.Headers.Authorization = await tokenProvider.GetTokenAsync(scopes, cancellationToken);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
