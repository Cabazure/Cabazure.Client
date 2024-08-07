﻿//HintName: Builder.IMessageRequestBuilder.g.cs
// <auto-generated/>
#nullable enable
using Microsoft.Extensions.Primitives;

namespace Cabazure.Client.Builder;

internal interface IMessageRequestBuilder
{
    IMessageRequestBuilder WithPathParameter(string name, string value);

    IMessageRequestBuilder WithQueryParameter(string name, string? value);

    IMessageRequestBuilder WithHeader(string name, StringValues value);

    IMessageRequestBuilder WithRequestOptions(IRequestOptions? options);

    IMessageRequestBuilder WithBody<TBody>(TBody body)
        where TBody : class;

    HttpRequestMessage Build(HttpMethod method);
}
#nullable disable