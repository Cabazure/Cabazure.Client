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
}
