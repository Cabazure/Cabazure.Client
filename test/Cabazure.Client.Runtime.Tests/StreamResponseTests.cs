using System.Net;

namespace Cabazure.Client.Runtime.Tests;

public class StreamResponseTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Create_Successful_Response(
        Stream okContent,
        string contentType,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new StreamResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: null,
            ContentObject: null,
            OkContent: okContent,
            ContentType: contentType,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.OkContent.Should().BeSameAs(okContent);
        response.ContentType.Should().Be(contentType);
        response.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Failed_Response_With_Null_OkContent(
        string content,
        object contentObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new StreamResponse(
            IsSuccess: false,
            StatusCode: HttpStatusCode.BadRequest,
            Content: content,
            ContentObject: contentObject,
            OkContent: null,
            ContentType: null,
            Headers: headers);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(contentObject);
        response.OkContent.Should().BeNull();
        response.ContentType.Should().BeNull();
    }

    [Fact]
    public void Dispose_Should_Dispose_Wrapped_HttpResponseMessage()
    {
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var httpResponseMessage = new HttpResponseMessage
        {
            Content = new StreamContent(stream),
        };

        var response = new StreamResponse(
            httpResponseMessage,
            true,
            HttpStatusCode.OK,
            null,
            null,
            null,
            null,
            new Dictionary<string, IEnumerable<string>>());

        response.Dispose();

        var act = () => stream.ReadByte();
        act.Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Dispose_Should_Not_Throw_When_No_HttpResponseMessage()
    {
        var response = new StreamResponse(
            true,
            HttpStatusCode.OK,
            null,
            null,
            null,
            null,
            new Dictionary<string, IEnumerable<string>>());

        var act = response.Dispose;

        act.Should().NotThrow();
    }
}
