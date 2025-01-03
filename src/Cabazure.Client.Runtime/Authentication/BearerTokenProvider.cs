﻿using System.Net.Http.Headers;
using Azure.Core;

namespace Cabazure.Client.Authentication
{
    public class BearerTokenProvider : IBearerTokenProvider
    {
        private readonly TokenRequestContext context;
        private readonly TokenCredential credential;
        private readonly IDateTimeProvider dateTimeProvider;
        private AccessToken accessToken;

        public BearerTokenProvider(
            TokenRequestContext context,
            TokenCredential credential,
            IDateTimeProvider dateTimeProvider)
        {
            this.context = context;
            this.credential = credential;
            this.dateTimeProvider = dateTimeProvider;
        }

        public async Task<AuthenticationHeaderValue> GetTokenAsync(
            CancellationToken cancellationToken)
        {
            if (TokenIsExpired())
            {
                accessToken = await credential.GetTokenAsync(context, cancellationToken);
            }

            return new AuthenticationHeaderValue(
                "Bearer",
                accessToken.Token);
        }

        private bool TokenIsExpired()
            => dateTimeProvider.GetDateTime().AddMinutes(1) > accessToken.ExpiresOn;
    }
}
