﻿//HintName: TestEndpoint.g.cs
// <auto-generated/>
using System.Net;
using Cabazure.Client;
using Cabazure.Client.Builder;

public partial class TestEndpoint : ITestEndpoint
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
        [Header("x-correlation-id")] string correlationId,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        var client = factory.CreateClient("ClientName");

        using var requestMessage = requestFactory
            .FromTemplate("ClientName", "/items")
            .WithHeader("x-correlation-id", correlationId)
            .WithRequestOptions(options)
            .Build(HttpMethod.Get);

        using var response = await client
            .WithRequestOptions(options)
            .SendAsync(requestMessage, cancellationToken);

        return await requestFactory
            .FromResponse("ClientName", response)
            .AddSuccessResponse<string[]>(HttpStatusCode.OK)
            .GetAsync(
                response => new EndpointResponse<string[]>(response),
                cancellationToken);
    }
}