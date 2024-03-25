using System.Net;
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
        Task<PagedResponse<string[]>> ExecuteAsync(
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

    [Theory, AutoNSubstituteData]
    public async Task Should_Set_Timeout_On_HttpClient(
        [Frozen] IHttpClientFactory factory,
        [Frozen, Substitute] HttpClient client,
        ListEndpoint sut,
        string item,
        ClientPaginationOptions options,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        _ = factory
            .CreateClient(default)
            .ReturnsForAnyArgs(client);
        options.Timeout = timeout;

        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        client.Timeout.Should().Be(timeout);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Configure_SuccessResponse(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
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
            .AddSuccessResponse<string[]>(HttpStatusCode.OK);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
        string item,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        _ = builder
            .Received(1)
            .GetAsync(
                Arg.Any<Func<EndpointResponse, PagedResponse<string[]>>>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
        PagedResponse<string[]> response,
        string item,
        ClientPaginationOptions options,
        CancellationToken cancellationToken)
    {
        builder
            .GetAsync(Arg.Any<Func<EndpointResponse, PagedResponse<string[]>>>(), cancellationToken)
            .ReturnsForAnyArgs(response);

        var result = await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        result
            .Should()
            .Be(response);
    }
}
