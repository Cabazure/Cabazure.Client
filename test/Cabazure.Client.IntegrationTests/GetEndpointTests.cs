using System.Net;
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
        ClientRequestOptions options,
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
        ClientRequestOptions options,
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
        ClientRequestOptions options,
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

    [Theory, AutoNSubstituteData]
    public async Task Should_Set_Timeout_On_HttpClient(
        [Frozen, Substitute] HttpClient client,
        GetEndpoint sut,
        string item,
        ClientRequestOptions options,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
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
        GetEndpoint sut,
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
        GetEndpoint sut,
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
        GetEndpoint sut,
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
