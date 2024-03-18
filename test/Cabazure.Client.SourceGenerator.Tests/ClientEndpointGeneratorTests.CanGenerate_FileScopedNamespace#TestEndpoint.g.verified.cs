﻿//HintName: TestEndpoint.g.cs
// <auto-generated/>
using System.Net;
using Cabazure.Client;
using Cabazure.Client.Builder;

namespace Test;

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
        ClientRequestOptions options,
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
                response => new EndpointResponse<string[]>(response),
                cancellationToken);
    }
}
