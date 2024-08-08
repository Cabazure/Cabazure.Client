﻿//HintName: Authentication.IBearerTokenProvider.g.cs
// <auto-generated/>
#nullable enable
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Cabazure.Client.Authentication
{
    internal interface IBearerTokenProvider
    {
        Task<AuthenticationHeaderValue> GetTokenAsync(
            CancellationToken cancellationToken);
    }
}
#nullable disable