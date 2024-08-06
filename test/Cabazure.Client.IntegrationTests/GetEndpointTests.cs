using System.Net;
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class GetEndpointTests
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items/{id}";

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
    internal void Should_Implement_Interface(
        GetEndpoint sut,
        string id,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
        => sut.Should().BeAssignableTo<IGetEndpoint>();

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
                && s.ServiceType == typeof(IGetEndpoint)
                && s.ImplementationType == typeof(GetEndpoint));
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_HttpClient(
        [Frozen] IHttpClientFactory factory,
        GetEndpoint sut,
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
        GetEndpoint sut,
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
        [Frozen] HttpClient client,
        [Frozen] HttpRequestMessage request,
        GetEndpoint sut,
        string id,
        ClientRequestOptions options,
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
        GetEndpoint sut,
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
    internal async Task Should_Add_ClientRequestOptions_To_Builder(
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
    internal async Task Should_Use_Correct_HttpMethod(
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
    internal async Task Should_Set_Timeout_On_HttpClient(
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
    internal async Task Should_Configure_SuccessResponse(
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
    internal async Task Should_Create_Result(
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
    internal async Task Should_Return_Result(
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
