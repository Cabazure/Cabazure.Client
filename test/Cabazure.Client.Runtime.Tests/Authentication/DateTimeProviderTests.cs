using Cabazure.Client.Authentication;

namespace Cabazure.Client.Runtime.Tests.Authentication;

public class DateTimeProviderTests
{
    [Theory, AutoNSubstituteData]
    public void GetDateTime_Returns_DateTimeOffset_UtcNow(
        DateTimeProvider sut)
        => sut
            .GetDateTime()
            .Should()
            .BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
}
