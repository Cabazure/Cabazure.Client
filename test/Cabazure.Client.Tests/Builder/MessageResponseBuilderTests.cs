using System.Net;
using AutoFixture.Xunit2;
using Cabazure.Client.Builder;

namespace Cabazure.Client.Tests.Builder;

public class MessageResponseBuilderTests
{
    [Theory, AutoNSubstituteData]
    public async Task IsSuccess_Should_Respect_Configured_ErrorResponse(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.NotFound;

        var result = await sut.AddErrorResponse(response.StatusCode)
            .GetAsync(res => res, cancellationToken);

        result
            .IsSuccess
            .Should()
            .BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public async Task IsSuccess_Should_Respect_Configured_SuccessResponse(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.NotFound;

        var result = await sut.AddSuccessResponse(response.StatusCode)
            .GetAsync(res => res, cancellationToken);

        result
            .IsSuccess
            .Should()
            .BeTrue();
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Deserialize_Configured_SuccessResponseCode(
        [Frozen] HttpResponseMessage response,
        [Frozen] IClientSerializer serializer,
        MessageResponseBuilder sut,
        DateTimeOffset expected,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        serializer
            .Deserialize<DateTimeOffset>(Arg.Any<string>(), Arg.Any<string>())
            .Returns(expected);

        var result = await sut.AddSuccessResponse<DateTimeOffset>(response.StatusCode)
            .GetAsync(res => res, cancellationToken);

        result
            .ContentObject
            .Should()
            .BeEquivalentTo(expected);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Deserialize_Configured_ErrorResponseCode(
        [Frozen] HttpResponseMessage response,
        [Frozen] IClientSerializer serializer,
        MessageResponseBuilder sut,
        DateTimeOffset expected,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.BadRequest;
        serializer
            .Deserialize<DateTimeOffset>(Arg.Any<string>(), Arg.Any<string>())
            .Returns(expected);

        var result = await sut.AddErrorResponse<DateTimeOffset>(response.StatusCode)
            .GetAsync(res => res, cancellationToken);

        result
            .ContentObject
            .Should()
            .BeEquivalentTo(expected);
    }

    [Theory, AutoNSubstituteData]
    public async Task Should_Return_Response_Headers(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        var expected = new Dictionary<string, IEnumerable<string>>
        {
            { "responseHeader", ["value"] },
            { "contentHeader", ["value"] },
        };
        response.Headers.Add("responseHeader", "value");
        response.Content.Headers.Add("contentHeader", "value");
        response.StatusCode = HttpStatusCode.OK;

        var result = await sut.AddSuccessResponse(response.StatusCode)
            .GetAsync(res => res, cancellationToken);

        result
            .Headers
            .Should()
            .BeEquivalentTo(expected);
    }
}