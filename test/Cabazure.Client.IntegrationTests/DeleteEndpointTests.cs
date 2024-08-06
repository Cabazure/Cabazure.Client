using System.Net;
using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Cabazure.Client.IntegrationTests;

public class DeleteEndpointTests
{
    public const string ClientName = "TestClient";
    public const string RouteTemplate = "/items/{id}";

    [ClientEndpoint(ClientName)]
    public interface IDeleteEndpoint
    {
        [Delete(RouteTemplate)]
        Task<EndpointResponse> ExecuteAsync(
            [Path] string id,
            CancellationToken cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Implement_Interface(
       DeleteEndpoint sut,
       string id,
       ClientRequestOptions options,
       CancellationToken cancellationToken)
       => sut.Should().BeAssignableTo<IDeleteEndpoint>();

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
                && s.ServiceType == typeof(IDeleteEndpoint)
                && s.ImplementationType == typeof(DeleteEndpoint));
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_HttpClient(
        [Frozen] IHttpClientFactory factory,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        factory
            .Received(1)
            .CreateClient(ClientName);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Request(
        [Frozen] IMessageRequestFactory requestFactory,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        requestFactory
            .Received(1)
            .FromTemplate(ClientName, RouteTemplate);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Send_Request(
        [Frozen] HttpClient client,
        [Frozen] HttpRequestMessage request,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        _ = client
            .Received(1)
            .SendAsync(request, Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Builder_From_Response(
        [Frozen] IMessageRequestFactory requestFactory,
        [Frozen] HttpResponseMessage response,
        DeleteEndpoint sut,
        string id,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            id,
            cancellationToken);

        requestFactory
            .Received(1)
            .FromResponse(ClientName, response);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Add_PathParameter(
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
    internal async Task Should_Use_Correct_HttpMethod(
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

    [Theory, AutoNSubstituteData]
    internal async Task Should_Configure_SuccessResponse(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        string item,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            cancellationToken);

        builder
            .Received(1)
            .AddSuccessResponse(HttpStatusCode.OK);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Create_Result(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        string item,
        CancellationToken cancellationToken)
    {
        await sut.ExecuteAsync(
            item,
            cancellationToken);

        _ = builder
            .Received(1)
            .GetAsync(cancellationToken);
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Return_Result(
        [Frozen] IMessageResponseBuilder builder,
        DeleteEndpoint sut,
        EndpointResponse response,
        string item,
        CancellationToken cancellationToken)
    {
        builder
            .GetAsync(cancellationToken)
            .ReturnsForAnyArgs(response);

        var result = await sut.ExecuteAsync(
            item,
            cancellationToken);

        result
            .Should()
            .Be(response);
    }
}
