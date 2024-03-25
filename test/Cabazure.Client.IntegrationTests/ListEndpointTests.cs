using Cabazure.Client.Builder;

namespace Cabazure.Client.IntegrationTests;

public class ListEndpointTests()
    : EndpointTests<ListEndpoint, ListEndpointTests.IListEndpoint>(
        ClientName,
        RouteTemplate)
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items";

    protected override Task ExecuteAsync(ListEndpoint endpoint)
        => endpoint.ExecuteAsync(default, default, default);

    [ClientEndpoint(ClientName)]
    public interface IListEndpoint
    {
        [Get(RouteTemplate)]
        Task<EndpointResponse<string[]>> ExecuteAsync(
            [Query("s")] string search,
            ClientPaginationOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_QueryParameter(
        [Frozen] IMessageRequestBuilder builder,
        ListEndpoint sut,
        string search,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            search,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithQueryParameter("s", search);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_ClientRequestOptions_To_Builder(
        [Frozen] IMessageRequestBuilder builder,
        ListEndpoint sut,
        string search,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            search,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithRequestOptions(options);
    }
}
