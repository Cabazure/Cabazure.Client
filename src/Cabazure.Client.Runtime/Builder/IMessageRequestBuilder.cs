using Microsoft.Extensions.Primitives;

namespace Cabazure.Client.Builder
{
    public interface IMessageRequestBuilder
    {
        IMessageRequestBuilder WithPathParameter(string name, string value);

        IMessageRequestBuilder WithQueryParameter(string name, string? value);

        IMessageRequestBuilder WithHeader(string name, StringValues value);

        IMessageRequestBuilder WithRequestOptions(IRequestOptions? options);

        IMessageRequestBuilder WithBody<TBody>(TBody body)
            where TBody : class;

        HttpRequestMessage Build(HttpMethod method);
    }
}
