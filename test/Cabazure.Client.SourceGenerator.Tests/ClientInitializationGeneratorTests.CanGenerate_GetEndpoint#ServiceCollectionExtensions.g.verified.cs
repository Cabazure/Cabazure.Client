//HintName: ServiceCollectionExtensions.g.cs
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddClientNameClient(
        this IServiceCollection services)
    {
        services.AddSingleton<ITestEndpoint, TestEndpoint>();

        return services;
    }
}
