using System.Net;
using Cabazure.Client.Builder;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class MessageResponseBuilderTests
{
    [Theory, AutoNSubstituteData]
    internal async Task IsSuccess_Should_Respect_Configured_ErrorResponse(
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
    internal async Task IsSuccess_Should_Respect_Configured_SuccessResponse(
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
    internal async Task Should_Deserialize_Configured_SuccessResponseCode(
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
    internal async Task Should_Deserialize_Configured_ErrorResponseCode(
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
    internal async Task Should_Return_Response_Headers(
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

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Expose_Stream_And_ContentType_On_Success(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("stream-content");
        response.Content.Headers.ContentType = new("application/octet-stream");

        var result = await sut.AddSuccessResponse(response.StatusCode)
            .GetStreamAsync(cancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.OkContent.Should().NotBeNull();
        result.ContentType.Should().Be("application/octet-stream");
        result.Content.Should().BeNull();

        using var reader = new StreamReader(result.OkContent!);
        (await reader.ReadToEndAsync()).Should().Be("stream-content");
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Read_Content_As_String_On_Error(
        [Frozen] HttpResponseMessage response,
        [Frozen] IClientSerializer serializer,
        MessageResponseBuilder sut,
        DateTimeOffset expected,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.BadRequest;
        response.Content = new StringContent("error-content");
        response.Content.Headers.ContentType = new("text/plain");
        serializer
            .Deserialize<DateTimeOffset>(Arg.Any<string>(), Arg.Any<string>())
            .Returns(expected);

        var result = await sut.AddErrorResponse<DateTimeOffset>(response.StatusCode)
            .GetStreamAsync(cancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.OkContent.Should().BeNull();
        result.ContentType.Should().Be("text/plain");
        result.Content.Should().Be("error-content");
        result.ContentObject.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetStreamAsync_Should_Return_Empty_Response_When_No_HttpResponseMessage()
    {
        var sut = new MessageResponseBuilder(
            null,
            Substitute.For<IClientSerializer>(),
            "ClientName");

        var result = await sut.GetStreamAsync(CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.OkContent.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Dispose_Response_On_Read_Failure(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        var content = new ThrowingContent();
        response.Content = content;

        var act = async () => await sut
            .AddSuccessResponse(response.StatusCode)
            .GetStreamAsync(cancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();

        content.Disposed.Should().BeTrue();
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Return_Raw_Stream_When_No_Timeout_Configured(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("stream-content");

        var result = await sut.AddSuccessResponse(response.StatusCode)
            .GetStreamAsync(cancellationToken);

        result.OkContent.Should().NotBeOfType<SlidingTimeoutStream>();
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Wrap_Stream_When_Timeout_Configured(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new StringContent("stream-content");
        using var timeoutCts = new CancellationTokenSource();

        var result = await sut
            .AddSuccessResponse(response.StatusCode)
            .WithStreamTimeout(timeoutCts, TimeSpan.FromSeconds(30))
            .GetStreamAsync(cancellationToken);

        result.OkContent.Should().BeOfType<SlidingTimeoutStream>();

        // Ownership of the CTS is transferred to the returned StreamResponse; disposing it
        // should dispose the CTS too (verified via ObjectDisposedException on further use).
        result.Dispose();
        var act = () => timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));
        act.Should().Throw<ObjectDisposedException>();
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Dispose_TimeoutCts_On_Error_Response(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.BadRequest;
        response.Content = new StringContent("error-content");
        using var timeoutCts = new CancellationTokenSource();

        await sut
            .AddErrorResponse(response.StatusCode)
            .WithStreamTimeout(timeoutCts, TimeSpan.FromSeconds(30))
            .GetStreamAsync(cancellationToken);

        var act = () => timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));
        act.Should().Throw<ObjectDisposedException>();
    }

    [Theory, AutoNSubstituteData]
    internal async Task GetStreamAsync_Should_Dispose_TimeoutCts_On_Read_Failure(
        [Frozen] HttpResponseMessage response,
        MessageResponseBuilder sut,
        CancellationToken cancellationToken)
    {
        response.StatusCode = HttpStatusCode.OK;
        response.Content = new ThrowingContent();
        using var timeoutCts = new CancellationTokenSource();

        var act = async () => await sut
            .AddSuccessResponse(response.StatusCode)
            .WithStreamTimeout(timeoutCts, TimeSpan.FromSeconds(30))
            .GetStreamAsync(cancellationToken);

        await act.Should().ThrowAsync<InvalidOperationException>();

        var disposedCheck = () => timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));
        disposedCheck.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public async Task GetStreamAsync_Should_Dispose_TimeoutCts_When_No_HttpResponseMessage()
    {
        var sut = new MessageResponseBuilder(
            null,
            Substitute.For<IClientSerializer>(),
            "ClientName");
        var timeoutCts = new CancellationTokenSource();

        await sut
            .WithStreamTimeout(timeoutCts, TimeSpan.FromSeconds(30))
            .GetStreamAsync(CancellationToken.None);

        var act = () => timeoutCts.CancelAfter(TimeSpan.FromSeconds(1));
        act.Should().Throw<ObjectDisposedException>();
    }

    private sealed class ThrowingContent : HttpContent
    {
        public bool Disposed { get; private set; }

        protected override Task<Stream> CreateContentReadStreamAsync()
            => throw new InvalidOperationException("Read failed.");

        protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            => throw new InvalidOperationException("Read failed.");

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => throw new InvalidOperationException("Read failed.");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}