using System.Net;

namespace Cabazure.Client.Runtime.Tests;

public class PagedResponseTests
{
    public class TestModel
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Paged_Response_With_ContinuationToken(
        string content,
        TestModel okContent,
        string continuationToken,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new PagedResponse<TestModel>(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: okContent,
            OkContent: okContent,
            ContinuationToken: continuationToken,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Should().Be(content);
        response.ContentObject.Should().Be(okContent);
        response.OkContent.Should().Be(okContent);
        response.ContinuationToken.Should().Be(continuationToken);
        response.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Paged_Response_Without_ContinuationToken(
        string content,
        TestModel okContent,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new PagedResponse<TestModel>(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: content,
            ContentObject: okContent,
            OkContent: okContent,
            ContinuationToken: null,
            Headers: headers);

        response.IsSuccess.Should().BeTrue();
        response.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Extract_ContinuationToken_From_Headers(
        TestModel model,
        string continuationToken)
    {
        var headers = new Dictionary<string, IEnumerable<string>>
        {
            { "x-continuation", new[] { continuationToken } }
        };

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.ContinuationToken.Should().Be(continuationToken);
        pagedResponse.OkContent.Should().BeSameAs(model);
        pagedResponse.ContentObject.Should().BeSameAs(model);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Return_Null_ContinuationToken_When_Header_Not_Present(
        TestModel model)
    {
        var headers = new Dictionary<string, IEnumerable<string>>
        {
            { "other-header", new[] { "value" } }
        };

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Return_Null_ContinuationToken_When_Headers_Is_Null(
        TestModel model)
    {
        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: null!);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Return_First_ContinuationToken_When_Multiple_Values(
        TestModel model,
        string firstToken,
        string secondToken)
    {
        var headers = new Dictionary<string, IEnumerable<string>>
        {
            { "x-continuation", new[] { firstToken, secondToken } }
        };

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.ContinuationToken.Should().Be(firstToken);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Cast_ContentObject_To_OkContent_When_Type_Matches(
        TestModel model)
    {
        var headers = new Dictionary<string, IEnumerable<string>>();

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.OkContent.Should().BeSameAs(model);
        pagedResponse.ContentObject.Should().BeSameAs(model);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Have_Null_OkContent_When_ContentObject_Type_Does_Not_Match(
        object wrongTypeObject)
    {
        var headers = new Dictionary<string, IEnumerable<string>>();

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: wrongTypeObject,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.OkContent.Should().BeNull();
        pagedResponse.ContentObject.Should().BeSameAs(wrongTypeObject);
    }

    [Fact]
    public void Should_Handle_Null_ContentObject_In_Conversion()
    {
        var headers = new Dictionary<string, IEnumerable<string>>();

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.NoContent,
            Content: null,
            ContentObject: null,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.OkContent.Should().BeNull();
        pagedResponse.ContentObject.Should().BeNull();
        pagedResponse.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Paged_Response_Should_Inherit_Base_Properties(
        TestModel okContent,
        string continuationToken,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new PagedResponse<TestModel>(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: okContent,
            OkContent: okContent,
            ContinuationToken: continuationToken,
            Headers: headers);

        response.IsOk.Should().BeTrue();
        response.IsNotFound.Should().BeFalse();
        response.IsTimeout.Should().BeFalse();
        response.IsBadRequest.Should().BeFalse();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Preserve_All_Properties_In_Conversion(
        TestModel model,
        string continuationToken)
    {
        var headers = new Dictionary<string, IEnumerable<string>>
        {
            { "x-continuation", new[] { continuationToken } },
            { "other-header", new[] { "value" } }
        };

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.IsSuccess.Should().BeTrue();
        pagedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        pagedResponse.Content.Should().Be("{}");
        pagedResponse.ContinuationToken.Should().Be(continuationToken);
        pagedResponse.Headers.Should().BeSameAs(headers);
    }

    [Theory, AutoNSubstituteData]
    public void Should_Create_Failed_Paged_Response(
        object errorObject,
        IReadOnlyDictionary<string, IEnumerable<string>> headers)
    {
        var response = new PagedResponse<TestModel>(
            IsSuccess: false,
            StatusCode: HttpStatusCode.BadRequest,
            Content: "error",
            ContentObject: errorObject,
            OkContent: null,
            ContinuationToken: null,
            Headers: headers);

        response.IsSuccess.Should().BeFalse();
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.OkContent.Should().BeNull();
        response.ContinuationToken.Should().BeNull();
    }

    [Theory, AutoNSubstituteData]
    public void Should_Handle_Empty_Continuation_Header_Value(
        TestModel model)
    {
        var headers = new Dictionary<string, IEnumerable<string>>
        {
            { "x-continuation", Array.Empty<string>() }
        };

        var baseResponse = new EndpointResponse(
            IsSuccess: true,
            StatusCode: HttpStatusCode.OK,
            Content: "{}",
            ContentObject: model,
            Headers: headers);

        var pagedResponse = new PagedResponse<TestModel>(baseResponse);

        pagedResponse.ContinuationToken.Should().BeNull();
    }
}
