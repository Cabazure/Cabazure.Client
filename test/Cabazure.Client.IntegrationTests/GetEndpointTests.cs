using Cabazure.Client.Builder;

namespace Cabazure.Client.IntegrationTests;

public class GetEndpointTests()
    : EndpointTests<GetEndpoint, GetEndpointTests.IGetEndpoint>(
        ClientName,
        RouteTemplate)
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items/{id}";

    protected override Task ExecuteAsync(GetEndpoint endpoint)
        => endpoint.ExecuteAsync(default, default, default);

    [ClientEndpoint(ClientName)]
    public interface IGetEndpoint
    {
        [Get(RouteTemplate)]
        Task<EndpointResponse<string>> ExecuteAsync(
            [Path("id")] string id,
            ClientRequestOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_PathParameter(
        [Frozen] IMessageRequestBuilder builder,
        GetEndpoint sut,
        string id,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithPathParameter("id", id);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_ClientRequestOptions_To_Builder(
        [Frozen] IMessageRequestBuilder builder,
        GetEndpoint sut,
        string id,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithRequestOptions(options);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Use_Correct_HttpMethod(
        [Frozen] IMessageRequestBuilder builder,
        GetEndpoint sut,
        string id,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        builder
            .Received(1)
            .Build(HttpMethod.Get);
    }
}
