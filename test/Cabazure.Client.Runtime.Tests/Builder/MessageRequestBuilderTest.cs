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
    internal void Should_Use_HttpVersion_20(
        HttpMethod method,
        MessageRequestBuilder sut)
        => sut.Build(method)
            .Version
            .Should()
            .Be(HttpVersion.Version20);

    [Theory, AutoNSubstituteData]
    internal void Should_Use_ApplicationJson_As_MediaType(
        MessageRequestBuilder sut)
    {
        var message = sut.Build(HttpMethod.Post);

        message
            .Headers
            .Accept
            .Should()
            .BeEquivalentTo(new[] { MediaTypeWithQualityHeaderValue.Parse("application/json") });

        message
            .Content
            .Headers
            .ContentType
            .Should()
            .Be(MediaTypeHeaderValue.Parse("application/json"));
    }

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

    [Theory, AutoNSubstituteData]
    internal void Should_Include_OnBehalfOf_Header(
        MessageRequestBuilder sut,
        ClientRequestOptions options)
    {
        sut.WithRequestOptions(options);

        var message = sut.Build(HttpMethod.Post);
        var result = message.Headers.Single(
                c => c.Key == "x-on-behalf-of")
            .Value;

        result
            .Should()
            .BeEquivalentTo(options.OnBehalfOf);
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
}