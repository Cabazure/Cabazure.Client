namespace Cabazure.Client.Tests;

public class EmbeddedSourceGeneratorTests
{
    [Fact]
    public Task CanGenerate_EmbeddedSource()
        => TestHelper.VerifyEmbeddedSource();
}
