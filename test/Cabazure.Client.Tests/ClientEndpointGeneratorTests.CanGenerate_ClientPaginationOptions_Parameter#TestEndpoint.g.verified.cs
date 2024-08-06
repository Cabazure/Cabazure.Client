﻿//HintName: TestEndpoint.g.cs
// <auto-generated/>
#nullable enable
using System.Net;
using Cabazure.Client;
using Cabazure.Client.Builder;

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

    public async Task<EndpointResponse<string[]>> ExecuteAsync(
        ClientPaginationOptions options)
    {
        var client = factory.CreateClient("ClientName");

        using var requestMessage = requestFactory
            .FromTemplate("ClientName", "/items")
            .WithRequestOptions(options)
            .Build(HttpMethod.Get);

        using var response = await client
            .WithRequestOptions(options)
            .SendAsync(requestMessage, CancellationToken.None);

        return await requestFactory
            .FromResponse("ClientName", response)
            .AddSuccessResponse<string[]>(HttpStatusCode.OK)
            .GetAsync(
                response => new EndpointResponse<string[]>(response),
                CancellationToken.None);
    }
}
#nullable disable
