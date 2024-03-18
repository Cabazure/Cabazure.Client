using Atc.Test;
using Cabazure.Client.IntegrationTests.Stub;
using FluentAssertions;

namespace Cabazure.Client.IntegrationTests;

public class TestEndpointTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Implement_Interface(
        TestEndpoint sut)
    {
        sut.Should().BeAssignableTo<ITestEndpoint>();
    }
}
