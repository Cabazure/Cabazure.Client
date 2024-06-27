using System.Text.Json;
using Cabazure.Client.Builder;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ReceivedExtensions;

namespace Cabazure.Client.IntegrationTests;

public abstract class EndpointTests<TClass, TInterface>(
    string clientName,
    string routeTemplate)
{
    protected abstract Task ExecuteAsync(TClass endpoint);

    [Theory, AutoNSubstituteData]
    public void Should_Implement_Interface(
        TClass sut)
        => sut.Should().BeAssignableTo<TInterface>();

    [Theory, AutoNSubstituteData]
    public void Should_Get_Registered_By_AddCabazureClient(
        ServiceCollection services,
        [Substitute] Action<JsonSerializerOptions> jsonOptions,
        [Substitute] Action<IHttpClientBuilder> builder)
    {
        services.AddCabazureClient(clientName, jsonOptions, builder);

        services
            .Should()
            .Contain(s
                => s.Lifetime == ServiceLifetime.Singleton
                && s.ServiceType == typeof(TInterface)
                && s.ImplementationType == typeof(TClass));
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_HttpClient(
        [Frozen] IHttpClientFactory factory,
        TClass sut)
    {
        await ExecuteAsync(sut);

        factory
            .Received(1)
            .CreateClient(clientName);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Request(
        [Frozen] IMessageRequestFactory requestFactory,
        TClass sut)
    {
        await ExecuteAsync(sut);

        requestFactory
            .Received(1)
            .FromTemplate(clientName, routeTemplate);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Send_Request(
        [Frozen] HttpClient client,
        [Frozen] HttpRequestMessage request,
        TClass sut)
    {
        await ExecuteAsync(sut);

        _ = client
            .Received(1)
            .SendAsync(request, Arg.Any<CancellationToken>());
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Create_Builder_From_Response(
        [Frozen] IMessageRequestFactory requestFactory,
        [Frozen] HttpResponseMessage response,
        TClass sut)
    {
        await ExecuteAsync(sut);

        requestFactory
            .Received(1)
            .FromResponse(clientName, response);
    }
}
