using FluentAssertions;

namespace Cabazure.Client.IntegrationTests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Should_Have_Generated_Implementation()
        => typeof(Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions)
            .GetMethods()
            .Select(m => m.Name)
            .Should()
            .Contain("AddTestStubClient");
}
