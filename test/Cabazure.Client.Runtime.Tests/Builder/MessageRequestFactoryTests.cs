using Cabazure.Client.Builder;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class MessageRequestFactoryTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Provide_MessageResponseBuilder_FromResponse(
        HttpResponseMessage response,
        string clientName,
        MessageRequestFactory sut)
        => sut.FromResponse(clientName, response)
            .Should()
            .NotBeNull();

    [Theory, AutoNSubstituteData]
    public void Should_Provide_MessageResponseBuilder_From_Null_Response(
        string clientName,
        MessageRequestFactory sut)
        => sut.FromResponse(clientName, null)
            .Should()
            .NotBeNull();

    [Theory, AutoNSubstituteData]
    public void Should_Provide_MessageRequestBuilder_FromTemplate(
        string template,
        string clientName,
        MessageRequestFactory sut)
        => sut.FromTemplate(clientName, template)
            .Should()
            .NotBeNull();
}