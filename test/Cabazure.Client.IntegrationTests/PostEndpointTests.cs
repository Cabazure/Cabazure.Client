using System.Net;
using Cabazure.Client.Builder;
using NSubstitute.ReceivedExtensions;

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
        Task<EndpointResponse<string>> ExecuteAsync(
            [Body] string item,
            ClientRequestOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Add_Body(
        [Frozen] IMessageRequestBuilder builder,
        PostEndpoint sut,
        string item,
        ClientRequestOptions options,
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
        ClientRequestOptions options,
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
        ClientRequestOptions options,
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

    [Theory, AutoNSubstituteData]
    public async Task Should_Set_Timeout_On_HttpClient(
        [Frozen] IHttpClientFactory factory,
        [Frozen, Substitute] HttpClient client,
        PostEndpoint sut,
        string item,
        ClientRequestOptions options,
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
        PostEndpoint sut,
        string item,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        builder
            .Received(1)
            .AddSuccessResponse<string>(HttpStatusCode.OK);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        PostEndpoint sut,
        string item,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            options,
            cancellationToken);

        _ = builder
            .Received(1)
            .GetAsync(
                Arg.Any<Func<EndpointResponse, EndpointResponse<string>>>(),
                cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        PostEndpoint sut,
        EndpointResponse<string> response,
        string item,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        builder
            .GetAsync(Arg.Any<Func<EndpointResponse, EndpointResponse<string>>>(), cancellationToken)
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
