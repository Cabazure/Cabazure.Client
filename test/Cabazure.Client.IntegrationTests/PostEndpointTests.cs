using Cabazure.Client.Builder;

namespace Cabazure.Client.IntegrationTests;

public class PostEndpointTests()
    : EndpointTests<PostEndpoint, PostEndpointTests.IPostEndpoint>(
        ClientName,
        RouteTemplate)
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items";

    protected override Task ExecuteAsync(PostEndpoint endpoint)
        => endpoint.ExecuteAsync(default, default, default);

    [ClientEndpoint(ClientName)]
    public interface IPostEndpoint
    {
        [Post(RouteTemplate)]
        Task<EndpointResponse<string[]>> ExecuteAsync(
            [Body] string item,
            ClientPaginationOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_Body(
        [Frozen] IMessageRequestBuilder builder,
        PostEndpoint sut,
        string item,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithBody(item);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_ClientRequestOptions_To_Builder(
        [Frozen] IMessageRequestBuilder builder,
        PostEndpoint sut,
        string item,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        builder
            .Received(1)
            .WithRequestOptions(options);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Use_Correct_HttpMethod(
        [Frozen] IMessageRequestBuilder builder,
        PostEndpoint sut,
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
            .Build(HttpMethod.Post);
    }
}
