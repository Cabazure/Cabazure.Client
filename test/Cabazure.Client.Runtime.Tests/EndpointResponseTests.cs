using System.Net;

namespace Cabazure.Client.Runtime.Tests;

public class EndpointResponseTests
{
    [Theory, AutoNSubstituteData]
    public void Should_Create_Successful_Response(
        string content,
        object contentObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: contentObject,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(contentObject);
        response.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Failed_Response(
        string content,
        object contentObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: false,
            StatusCode: HttpStatusCode.BadRequest,
            Content: content,
            ContentObject: contentObject,
            Headers: headers);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(contentObject);
        response.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Handle_Null_Content(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.NoContent,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        response.Content.Should().BeNull();
        response.ContentObject.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void IsOk_Should_Return_True_For_OK_Status(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsOk.Should().BeTrue();
    }

    [Theory]
    [InlineAutoNSubstituteData(HttpStatusCode.Created)]
    [InlineAutoNSubstituteData(HttpStatusCode.Accepted)]
    [InlineAutoNSubstituteData(HttpStatusCode.NoContent)]
    [InlineAutoNSubstituteData(HttpStatusCode.BadRequest)]
    [InlineAutoNSubstituteData(HttpStatusCode.NotFound)]
    public void IsOk_Should_Return_False_For_Non_OK_Status(
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: statusCode is HttpStatusCode.Created or HttpStatusCode.Accepted or HttpStatusCode.NoContent,
            StatusCode: statusCode,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsOk.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void IsNotFound_Should_Return_True_For_NotFound_Status(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: false,
            StatusCode: HttpStatusCode.NotFound,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsNotFound.Should().BeTrue();
    }

    [Theory]
    [InlineAutoNSubstituteData(HttpStatusCode.OK)]
    [InlineAutoNSubstituteData(HttpStatusCode.BadRequest)]
    [InlineAutoNSubstituteData(HttpStatusCode.InternalServerError)]
    public void IsNotFound_Should_Return_False_For_Other_Status(
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: statusCode is HttpStatusCode.OK,
            StatusCode: statusCode,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsNotFound.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void IsTimeout_Should_Return_True_For_GatewayTimeout_Status(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: false,
            StatusCode: HttpStatusCode.GatewayTimeout,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsTimeout.Should().BeTrue();
    }

    [Theory]
    [InlineAutoNSubstituteData(HttpStatusCode.OK)]
    [InlineAutoNSubstituteData(HttpStatusCode.RequestTimeout)]
    [InlineAutoNSubstituteData(HttpStatusCode.BadGateway)]
    public void IsTimeout_Should_Return_False_For_Other_Status(
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: statusCode is HttpStatusCode.OK,
            StatusCode: statusCode,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsTimeout.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void IsBadRequest_Should_Return_True_For_BadRequest_Status(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: false,
            StatusCode: HttpStatusCode.BadRequest,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsBadRequest.Should().BeTrue();
    }

    [Theory]
    [InlineAutoNSubstituteData(HttpStatusCode.OK)]
    [InlineAutoNSubstituteData(HttpStatusCode.NotFound)]
    [InlineAutoNSubstituteData(HttpStatusCode.InternalServerError)]
    public void IsBadRequest_Should_Return_False_For_Other_Status(
        HttpStatusCode statusCode,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse(
            IsSuccess: statusCode is HttpStatusCode.OK,
            StatusCode: statusCode,
            Content: null,
            ContentObject: null,
            Headers: headers);

        response.IsBadRequest.Should().BeFalse();
    }
}

public class EndpointResponseGenericTests
{
    public class TestModel
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Successful_Typed_Response(
        string content,
        TestModel okContent,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse<TestModel>(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: okContent,
            OkContent: okContent,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(okContent);
        response.OkContent.Should().Be(okContent);
        response.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Failed_Typed_Response_With_Null_OkContent(
        string content,
        object contentObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse<TestModel>(
            IsSuccess: false,
            StatusCode: HttpStatusCode.BadRequest,
            Content: content,
            ContentObject: contentObject,
            OkContent: null,
            Headers: headers);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(contentObject);
        response.OkContent.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Cast_ContentObject_To_OkContent_When_Type_Matches(
        string content,
        TestModel model,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: model,
            Headers: headers);

        var typedResponse = new EndpointResponse<TestModel>(baseResponse);

        typedResponse.OkContent.Should().BeSameAs(model);
        typedResponse.ContentObject.Should().BeSameAs(model);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Have_Null_OkContent_When_ContentObject_Type_Does_Not_Match(
        string content,
        object wrongTypeObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: wrongTypeObject,
            Headers: headers);

        var typedResponse = new EndpointResponse<TestModel>(baseResponse);

        typedResponse.OkContent.Should().BeNull();
        typedResponse.ContentObject.Should().BeSameAs(wrongTypeObject);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Handle_Null_ContentObject_In_Conversion(
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.NoContent,
            Content: null,
            ContentObject: null,
            Headers: headers);

        var typedResponse = new EndpointResponse<TestModel>(baseResponse);

        typedResponse.OkContent.Should().BeNull();
        typedResponse.ContentObject.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Typed_Response_Should_Inherit_Base_Properties(
        string content,
        TestModel okContent,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new EndpointResponse<TestModel>(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: okContent,
            OkContent: okContent,
            Headers: headers);

        response.IsOk.Should().BeTrue();
        response.IsNotFound.Should().BeFalse();
        response.IsTimeout.Should().BeFalse();
        response.IsBadRequest.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Preserve_Headers_In_Conversion(
        TestModel model,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var typedResponse = new EndpointResponse<TestModel>(baseResponse);

        typedResponse.Headers.Should().BeSameAs(headers);
    }
}
