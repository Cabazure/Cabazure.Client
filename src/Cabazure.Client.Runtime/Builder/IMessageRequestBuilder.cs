using Microsoft.Extensions.Primitives;

namespace Cabazure.Client.Builder
{
    public interface IMessageRequestBuilder
    {
        IMessageRequestBuilder WithPathParameter(string name, string value);

        IMessageRequestBuilder WithQueryParameter(string name, string? value);

        /// <summary>
        /// Sets a general request header.
        /// </summary>
        /// <remarks>
        /// Names are case-insensitive. Non-empty values replace previous values; empty values are ignored.
        /// </remarks>
        IMessageRequestBuilder WithHeader(string name, StringValues value);

        /// <summary>
        /// Adds a content header when a body is supplied through <see cref="WithBody{TBody}"/>.
        /// Content headers are ignored when the request has no content.
        /// </summary>
        /// <remarks>Uses the same replacement rules as <see cref="WithHeader"/>.</remarks>
        IMessageRequestBuilder WithContentHeader(string name, StringValues value);

        IMessageRequestBuilder WithRequestOptions(IRequestOptions? options);

        IMessageRequestBuilder WithBody<TBody>(TBody body)
            where TBody : class;

        HttpRequestMessage Build(HttpMethod method);
    }
}
