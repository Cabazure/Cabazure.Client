using System.Net;
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class ListEndpointTests
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items";

    [ClientEndpoint(ClientName)]
    public interface IListEndpoint
    {
        [Get(RouteTemplate)]
        Task<PagedResponse<string[]>> ExecuteAsync(
            [Query("s")] string search,
            PagedRequestOptions options,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Implement_Interface(
       ListEndpoint sut,
       string id,
       ClientRequestOptions options,
       CancellationToken cancellationToken)
       => sut.Should().BeAssignableTo<IListEndpoint>();

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
                && s.ServiceType == typeof(IListEndpoint)
                && s.ImplementationType == typeof(ListEndpoint));
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_HttpClient(
        [Frozen] IHttpClientFactory factory,
        ListEndpoint sut,
        string id,
        PagedRequestOptions options,
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
        ListEndpoint sut,
        string id,
        PagedRequestOptions options,
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
        [Frozen] HttpClient client,
        [Frozen] HttpRequestMessage request,
        ListEndpoint sut,
        string id,
        PagedRequestOptions options,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            options,
            cancellationToken);

        _ = client
            .Received(1)
            .SendAsync(request, Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Builder_From_Response(
        [Frozen] IMessageRequestFactory requestFactory,
        [Frozen] HttpResponseMessage response,
        ListEndpoint sut,
        string id,
        PagedRequestOptions options,
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
    internal async Task Should_Add_QueryParameter(
        [Frozen] IMessageRequestBuilder builder,
        ListEndpoint sut,
        string search,
        PagedRequestOptions options,
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
    internal async Task Should_Add_PagedRequestOptions_To_Builder(
        [Frozen] IMessageRequestBuilder builder,
        ListEndpoint sut,
        string search,
        PagedRequestOptions options,
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
    internal async Task Should_Set_Timeout_On_HttpClient(
        [Frozen] IHttpClientFactory factory,
        [Frozen, Substitute] HttpClient client,
        ListEndpoint sut,
        string item,
        PagedRequestOptions options,
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
    internal async Task Should_Configure_SuccessResponse(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
        string item,
        PagedRequestOptions options,
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
    internal async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
        string item,
        PagedRequestOptions options,
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
    internal async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        ListEndpoint sut,
        PagedResponse<string[]> response,
        string item,
        PagedRequestOptions options,
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
