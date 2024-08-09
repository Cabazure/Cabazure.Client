﻿//HintName: TestEndpoint.g.cs
// <auto-generated/>
#nullable enable
using System.Net;
using System.Net.Http;
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

    public async Task<EndpointResponse> ExecuteAsync(
        string id,
        CancellationToken cancellationToken)
    {
        var client = factory.CreateClient("ClientName");

        using var requestMessage = requestFactory
            .FromTemplate("ClientName", "/items/{id}")
            .WithPathParameter("id", id)
            .Build(HttpMethod.Delete);

        using var response = await client
            .SendAsync(requestMessage, cancellationToken);

        return await requestFactory
            .FromResponse("ClientName", response)
            .AddSuccessResponse(HttpStatusCode.OK)
            .GetAsync(cancellationToken);
    }
}
#nullable disable
