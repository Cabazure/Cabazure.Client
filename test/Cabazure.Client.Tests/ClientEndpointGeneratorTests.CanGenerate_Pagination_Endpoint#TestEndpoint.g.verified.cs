﻿//HintName: TestEndpoint.g.cs
// <auto-generated/>
#nullable enable
using System.Net;
using System.Net.Http;
using Cabazure.Client;
using Cabazure.Client.Builder;

namespace Test
{
    internal partial class TestEndpoint : ITestEndpoint
    {
        private readonly IHttpClientFactory factory;
        private readonly IMessageRequestFactory requestFactory;

        public TestEndpoint(
            IHttpClientFactory factory,
            IMessageRequestFactory requestFactory)
        {
            this.factory = factory;
            this.requestFactory = requestFactory;
        }

        public async Task<PagedResponse<string[]>> ExecuteAsync(
        PagedRequestOptions options,
        CancellationToken cancellationToken)
        {
            var client = factory.CreateClient("ClientName");

            using var requestMessage = requestFactory
                .FromTemplate("ClientName", "/routes")
                .WithRequestOptions(options)
                .Build(HttpMethod.Get);

            using var response = await client
                .WithRequestOptions(options)
                .SendAsync(requestMessage, cancellationToken);

            return await requestFactory
                .FromResponse("ClientName", response)
                .AddSuccessResponse<string[]>(HttpStatusCode.OK)
                .GetAsync(
                    response => new PagedResponse<string[]>(response),
                    cancellationToken);
        }
    }
}
#nullable disable
