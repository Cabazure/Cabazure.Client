using System.Net;
using Cabazure.Client.Builder;

namespace Cabazure.Client.IntegrationTests;

public class DeleteEndpointTests()
    : EndpointTests<DeleteEndpoint, DeleteEndpointTests.IDeleteEndpoint>(
        ClientName,
        RouteTemplate)
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items/{id}";

    protected override Task ExecuteAsync(DeleteEndpoint endpoint)
        => endpoint.ExecuteAsync(default, default);

    [ClientEndpoint(ClientName)]
    public interface IDeleteEndpoint
    {
        [Delete(RouteTemplate)]
        Task<EndpointResponse> ExecuteAsync(
            [Path] string id,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_PathParameter(
        [Frozen] IMessageRequestBuilder builder,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        builder
            .Received(1)
            .WithPathParameter("id", id);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Use_Correct_HttpMethod(
        [Frozen] IMessageRequestBuilder builder,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        builder
            .Received(1)
            .Build(HttpMethod.Delete);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Configure_SuccessResponse(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        string item,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            cancellationToken);

        builder
            .Received(1)
            .AddSuccessResponse(HttpStatusCode.OK);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        string item,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            cancellationToken);

        _ = builder
            .Received(1)
            .GetAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        EndpointResponse response,
        string item,
        CancellationToken cancellationToken)
    {
        builder
            .GetAsync(cancellationToken)
            .ReturnsForAnyArgs(response);

        var result = await sut.ExecuteAsync(
            item,
            cancellationToken);

        result
            .Should()
            .Be(response);
    }
}
