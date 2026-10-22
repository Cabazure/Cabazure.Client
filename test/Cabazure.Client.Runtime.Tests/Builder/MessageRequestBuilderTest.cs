using System.Net;
using System.Net.Http.Headers;
using Cabazure.Client.Builder;
using Microsoft.Extensions.Primitives;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class MessageRequestBuilderTest
{
    [Theory, AutoNSubstituteData]
    internal void Should_Use_HttpMethod(
        HttpMethod method,
        MessageRequestBuilder sut)
        => sut.Build(method)
            .Method
            .Should()
            .Be(method);

    [Theory, AutoNSubstituteData]
    internal void Should_Use_Http2_As_Default_Version(
        HttpMethod method,
        MessageRequestBuilder sut)
        => sut.Build(method)
            .Version
            .Should()
            .Be(HttpVersion.Version20);

    [Theory, AutoNSubstituteData]
    internal void Should_Use_ApplicationJson_As_Accept(
        HttpMethod method,
        MessageRequestBuilder sut)
        => sut.Build(method)
            .Headers
            .Accept
            .Should()
            .BeEquivalentTo([MediaTypeWithQualityHeaderValue.Parse("application/json")]);

    [Theory, AutoNSubstituteData]
    internal void Should_Use_ApplicationJson_As_ContentType_When_Body_Is_Set(
        [Frozen] IClientSerializer serializer,
        string body,
        MessageRequestBuilder sut)
    {
        serializer.Serialize(Arg.Any<string>(), Arg.Any<object>()).Returns(body);
        sut.WithBody(body);

        sut.Build(HttpMethod.Post)
            .Content!
            .Headers
            .ContentType
            .Should()
            .Be(MediaTypeHeaderValue.Parse("application/json"));
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Not_Set_Content_When_No_Body(
        HttpMethod method,
        MessageRequestBuilder sut)
        => sut.Build(method)
            .Content
            .Should()
            .BeNull();

    [Theory]
    [InlineAutoNSubstituteData("/api/chargepoints/{chargepointId}/connectors/{connectorId}/startcharging")]
    internal void Should_Replace_Path_Parameters(
        string template,
        string parameterValue,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithPathParameter("chargepointId", parameterValue);
        sut.WithPathParameter("connectorId", parameterValue);
        var message = sut.Build(HttpMethod.Post);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/chargepoints/{parameterValue}/connectors/{parameterValue}/startcharging");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/chargepoints/1232/reset")]
    internal void Should_Replace_Query_Parameters(
        string template,
        string parameterValue,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);

        sut.WithQueryParameter("type", parameterValue);
        sut.WithQueryParameter("delay", parameterValue);
        var message = sut.Build(HttpMethod.Post);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/chargepoints/1232/reset?type={parameterValue}&delay={parameterValue}");
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Include_BodyAsync(
        string template,
        IClientSerializer serializer,
        string clientName,
        string content)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        serializer
            .Serialize(clientName, Arg.Any<object>())
            .Returns(content);

        sut.WithBody(content);

        var message = sut.Build(HttpMethod.Post);
        var result = await message.Content.ReadAsStringAsync();

        result
            .Should()
            .Be(content);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Include_Header(
        MessageRequestBuilder sut,
        string headerName,
        string headerValue)
    {
        sut.WithHeader(headerName, headerValue);

        var message = sut.Build(HttpMethod.Post);
        var result = message.Headers.Single(
                c => c.Key == headerName)
            .Value
            .Single();

        result
            .Should()
            .Be(headerValue);
    }

    [Theory]
    [InlineAutoNSubstituteData("Allow", "GET")]
    [InlineAutoNSubstituteData("Content-Disposition", "attachment")]
    [InlineAutoNSubstituteData("Content-Encoding", "gzip")]
    [InlineAutoNSubstituteData("Content-Language", "en")]
    [InlineAutoNSubstituteData("Content-Length", "12")]
    [InlineAutoNSubstituteData("Content-Location", "https://example.test/")]
    [InlineAutoNSubstituteData("Content-MD5", "1B2M2Y8AsgTpgAmY7PhCfg==")]
    [InlineAutoNSubstituteData("Content-Range", "bytes 0-11/12")]
    [InlineAutoNSubstituteData("content-type", "application/custom+json")]
    [InlineAutoNSubstituteData("Expires", "Wed, 21 Oct 2015 07:28:00 GMT")]
    [InlineAutoNSubstituteData("Last-Modified", "Wed, 21 Oct 2015 07:28:00 GMT")]
    internal void Should_Route_ContentHeaders_To_Content(
        string name,
        string value,
        [Frozen] IClientSerializer serializer,
        MessageRequestBuilder sut)
    {
        serializer.Serialize(Arg.Any<string>(), Arg.Any<object>()).Returns("payload");
        sut.WithBody("payload");
        sut.WithContentHeader(name, value);

        using var message = sut.Build(HttpMethod.Post);

        message.Content!.Headers.GetValues(name).Should().Equal(value);
        message.Headers.Should().NotContain(h => h.Key.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Preserve_Multiple_ContentHeader_Values(
        [Frozen] IClientSerializer serializer,
        MessageRequestBuilder sut)
    {
        serializer.Serialize(Arg.Any<string>(), Arg.Any<object>()).Returns("payload");
        sut.WithBody("payload");
        sut.WithContentHeader("Content-Language", new StringValues(["en", "da"]));

        using var message = sut.Build(HttpMethod.Post);

        message.Content!.Headers.ContentLanguage.Should().Equal("en", "da");
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Skip_ContentHeaders_When_No_Body_Is_Supplied(
        MessageRequestBuilder sut)
    {
        sut.WithContentHeader("Content-Type", "application/custom+json");
        sut.WithContentHeader("Content-Language", "en");
        sut.WithHeader("x-operation", "get");

        using var message = sut.Build(HttpMethod.Get);

        message.Content.Should().BeNull();
        message.Headers.GetValues("x-operation").Should().Equal("get");
        message.Headers.Should().NotContain(h => h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
            || h.Key.Equals("Content-Language", StringComparison.OrdinalIgnoreCase));
    }

    [Theory, AutoNSubstituteData]
    internal async Task Should_Preserve_ExplicitBody_When_Serialization_Is_Empty(
        [Frozen] IClientSerializer serializer,
        MessageRequestBuilder sut)
    {
        serializer.Serialize(Arg.Any<string>(), Arg.Any<object>()).Returns(string.Empty);
        sut.WithBody("payload");
        sut.WithContentHeader("Content-Type", "application/custom+json");

        using var message = sut.Build(HttpMethod.Post);

        message.Content.Should().NotBeNull();
        message.Content!.Headers.ContentType!.MediaType.Should().Be("application/custom+json");
        (await message.Content.ReadAsStringAsync(TestContext.Current.CancellationToken))
            .Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Omit_Empty_ContentHeaders(
        [Frozen] IClientSerializer serializer,
        MessageRequestBuilder sut)
    {
        serializer.Serialize(Arg.Any<string>(), Arg.Any<object>()).Returns("payload");
        sut.WithBody("payload");
        sut.WithContentHeader("Content-Type", string.Empty);
        sut.WithContentHeader("Content-Language", StringValues.Empty);
        sut.WithContentHeader("Content-Encoding", " ");

        using var message = sut.Build(HttpMethod.Post);

        message.Content!.Headers.ContentType!.MediaType.Should().Be("application/json");
        message.Content.Headers.ContentLanguage.Should().BeEmpty();
        message.Content.Headers.ContentEncoding.Should().BeEmpty();
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Include_CorrelationId_Header(
        MessageRequestBuilder sut,
        ClientRequestOptions options)
    {
        sut.WithRequestOptions(options);

        var message = sut.Build(HttpMethod.Post);
        var result = message.Headers.Single(
                c => c.Key == "x-correlation-id")
            .Value
            .Single();

        result
            .Should()
            .Be(options.CorrelationId);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Include_MacItemCount_Header(
        MessageRequestBuilder sut,
        PagedRequestOptions options)
    {
        sut.WithRequestOptions(options);

        var message = sut.Build(HttpMethod.Post);
        var result = message.Headers.Single(
                c => c.Key == "x-max-item-count")
            .Value
            .Single();

        result
            .Should()
            .Be(options.MaxItemCount.ToString());
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Include_Continuation_Header(
        MessageRequestBuilder sut,
        PagedRequestOptions options)
    {
        sut.WithRequestOptions(options);

        var message = sut.Build(HttpMethod.Post);
        var result = message.Headers.Single(
                c => c.Key == "x-continuation")
            .Value
            .Single();

        result
            .Should()
            .Be(options.ContinuationToken);
    }

    [Theory, AutoNSubstituteData]
    internal void Should_Ignore_Header_When_Null_Or_Empty_Or_Whitespace(
        MessageRequestBuilder sut,
        string headerName)
    {
        sut.WithHeader(headerName, " ");
        sut.WithHeader(headerName, StringValues.Empty);
        sut.WithHeader(headerName, string.Empty);
        sut.Build(HttpMethod.Post)
            .Headers.Any(c => c.Key == headerName)
            .Should()
            .BeFalse();
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/users/{userId}/posts/{postId}")]
    internal void Should_Replace_Multiple_Path_Parameters_Correctly(
        string template,
        string userId,
        string postId,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithPathParameter("userId", userId);
        sut.WithPathParameter("postId", postId);

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/users/{userId}/posts/{postId}");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/search")]
    internal void Should_Append_Multiple_Query_Parameters_Correctly(
        string template,
        string searchTerm,
        string category,
        string sortOrder,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithQueryParameter("q", searchTerm);
        sut.WithQueryParameter("category", category);
        sut.WithQueryParameter("sort", sortOrder);

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/search?q={searchTerm}&category={category}&sort={sortOrder}");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/users/{userId}/posts")]
    internal void Should_Handle_Mix_Of_Path_And_Query_Parameters(
        string template,
        string userId,
        string status,
        string limit,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithPathParameter("userId", userId);
        sut.WithQueryParameter("status", status);
        sut.WithQueryParameter("limit", limit);

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/users/{userId}/posts?status={status}&limit={limit}");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/users/{userId}/profile")]
    internal void Should_URL_Encode_Special_Characters_In_Path_Parameters(
        string template,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithPathParameter("userId", "user@test.com");

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be("/api/users/user%40test.com/profile");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/search")]
    internal void Should_URL_Encode_Special_Characters_In_Query_Parameters(
        string template,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithQueryParameter("q", "hello world");
        sut.WithQueryParameter("tags", "C# & .NET");

        var message = sut.Build(HttpMethod.Get);

        var uri = message.RequestUri.ToString();
        uri.Should().Contain("q=hello%20world");
        uri.Should().Contain("tags=C%23%20%26%20.NET");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/search")]
    internal void Should_Omit_Null_Query_Parameter(
        string template,
        string validValue,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithQueryParameter("param1", validValue);
        sut.WithQueryParameter("param2", null);
        sut.WithQueryParameter("param3", validValue);

        var message = sut.Build(HttpMethod.Get);

        var uri = message.RequestUri.ToString();
        uri.Should().Contain("param1");
        uri.Should().NotContain("param2");
        uri.Should().Contain("param3");
        uri.Should().Be($"/api/search?param1={validValue}&param3={validValue}");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/data")]
    internal void Should_Handle_Empty_String_Query_Parameter(
        string template,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithQueryParameter("filter", string.Empty);

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be("/api/data?filter=");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/base?existing=value")]
    internal void Should_Append_Query_Parameters_To_Existing_Query_String(
        string template,
        string newParam,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithQueryParameter("added", newParam);

        var message = sut.Build(HttpMethod.Get);

        message
            .RequestUri
            .ToString()
            .Should()
            .Be($"/api/base?existing=value&added={newParam}");
    }

    [Theory]
    [InlineAutoNSubstituteData("/api/users/{id}/items/{itemId}")]
    internal void Should_Handle_Complex_Scenario_With_Multiple_Path_And_Query_Parameters(
        string template,
        IClientSerializer serializer,
        string clientName)
    {
        var sut = new MessageRequestBuilder(template, serializer, clientName);
        sut.WithPathParameter("id", "123");
        sut.WithPathParameter("itemId", "456");
        sut.WithQueryParameter("include", "details");
        sut.WithQueryParameter("format", "json");
        sut.WithQueryParameter("optional", null);

        var message = sut.Build(HttpMethod.Get);

        var uri = message.RequestUri.ToString();
        uri.Should().Be("/api/users/123/items/456?include=details&format=json");
        uri.Should().NotContain("optional");
    }
}