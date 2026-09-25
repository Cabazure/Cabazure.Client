using System.Net;
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

[CustomizeWith(typeof(DownloadHttpClientCustomization))]
public class DownloadEndpointTests
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items/{id}";

    [ClientEndpoint(ClientName)]
    public interface IDownloadEndpoint
    {
        [Get(RouteTemplate)]
        Task<StreamResponse> ExecuteAsync(
            [Path("id")] string id,
            ClientRequestOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Implement_Interface(
        DownloadEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
        => sut.Should().BeAssignableTo<IDownloadEndpoint>();

    [Theory, AutoNSubstituteData]
    public void Should_Get_Registered_By_AddCabazureClient(
        ServiceCollection services,
        [Substitute] Action<JsonSerializerOptions> jsonOptions,
        [Substitute] Action<IHttpClientBuilder> builder)
    {
        services.AddCabazureClient(ClientName, jsonOptions, builder);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(IDownloadEndpoint)
                && s.ImplementationType == typeof(DownloadEndpoint));
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_HttpClient(
        [Frozen] IHttpClientFactory factory,
        DownloadEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        factory
            .Received(1)
            .CreateClient(ClientName);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Request(
        [Frozen] IMessageRequestFactory requestFactory,
        DownloadEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        requestFactory
            .Received(1)
            .FromTemplate(ClientName, RouteTemplate);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Send_Request(
        [Frozen] HttpMessageHandler handler,
        [Frozen] HttpRequestMessage request,
        DownloadEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        _ = handler
            .Received(1)
            .InvokeProtectedAsync<HttpResponseMessage>(
                "SendAsync",
                request,
                Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Builder_From_Response(
        [Frozen] IMessageRequestFactory requestFactory,
        [Frozen] HttpResponseMessage response,
        DownloadEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        requestFactory
            .Received(1)
            .FromResponse(ClientName, response);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Add_PathParameter(
        [Frozen] IMessageRequestBuilder builder,
        DownloadEndpoint sut,
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
    internal async Task Should_Add_ClientRequestOptions_To_Builder(
        [Frozen] IMessageRequestBuilder builder,
        DownloadEndpoint sut,
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
    internal async Task Should_Use_Correct_HttpMethod(
        [Frozen] IMessageRequestBuilder builder,
        DownloadEndpoint sut,
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
    internal async Task Should_Configure_SuccessResponse(
        [Frozen] IMessageResponseBuilder builder,
        DownloadEndpoint sut,
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
            .AddSuccessResponse(HttpStatusCode.OK);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        DownloadEndpoint sut,
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
            .GetStreamAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        DownloadEndpoint sut,
        StreamResponse response,
        string item,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
    {
        builder
            .GetStreamAsync(cancellationToken)
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
