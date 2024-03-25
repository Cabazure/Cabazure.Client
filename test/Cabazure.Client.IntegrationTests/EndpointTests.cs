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
    public void Should_Get_Registered_By_ConfigureClient(
        ServiceCollection services,
        [Substitute] Action<IHttpClientBuilder> builder,
        [Substitute] Action<JsonSerializerOptions> jsonOptions)
    {
        services.ConfigureClient(clientName, builder, jsonOptions);

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
        [Frozen] IHttpClientFactory factory,
        [Frozen] IMessageRequestFactory requestFactory,
        TClass sut)
    {
        await ExecuteAsync(sut);

        requestFactory
            .Received(1)
            .FromTemplate(clientName, routeTemplate);
    }
}
