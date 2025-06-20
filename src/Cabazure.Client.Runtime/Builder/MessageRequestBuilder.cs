using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Cabazure.Client.Builder
{
    public class MessageRequestBuilder : IMessageRequestBuilder
    {
        private readonly Dictionary<string, string> pathMapper = new();
        private readonly Dictionary<string, string> queryMapper = new();
        private readonly Dictionary<string, StringValues> headerMapper = new();
        private readonly string template;
        private readonly IClientSerializer serializer;
        private readonly string clientName;
        private string content = string.Empty;
        private IRequestOptions? options;

        public MessageRequestBuilder(
            string template,
            IClientSerializer serializer,
            string clientName)
        {
            this.template = template;
            this.serializer = serializer;
            this.clientName = clientName;
        }

        public HttpRequestMessage Build(HttpMethod method)
        {
            var message = new HttpRequestMessage();

            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            message.RequestUri = BuildRequestUri();
            message.Content = new StringContent(content);
            message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            message.Method = method;
            message.Version = new Version(2, 0);

            foreach (var header in headerMapper)
            {
                message.Headers.Add(header.Key, header.Value.ToArray());
            }

            if (options is not null)
            {
                options.ConfigureHttpRequest(message);
            }

            return message;
        }

        public IMessageRequestBuilder WithHeader(string name, StringValues value)
        {
            if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value))
            {
                headerMapper[name] = value;
            }

            return this;
        }

        public IMessageRequestBuilder WithRequestOptions(
            IRequestOptions? options)
        {
            this.options = options;
            return this;
        }

        public IMessageRequestBuilder WithBody<TBody>(TBody body)
            where TBody : class
        {
            content = serializer.Serialize(clientName, body);

            return this;
        }

        public IMessageRequestBuilder WithPathParameter(string name, string value)
        {
            pathMapper[name] = value;

            return this;
        }

        public IMessageRequestBuilder WithQueryParameter(string name, string? value)
        {
            if (value is not null)
            {
                queryMapper[name] = value;
            }

            return this;
        }

        private Uri BuildRequestUri()
        {
            var urlBuilder = new StringBuilder();

            urlBuilder.Append(template);
            foreach (var parameter in pathMapper)
            {
                urlBuilder.Replace($"{{{parameter.Key}}}", Uri.EscapeDataString(parameter.Value));
            }

            if (queryMapper.Count != 0)
            {
                urlBuilder.Append(template.Contains('?') ? '&' : '?');
                urlBuilder.Append(string.Join("&", queryMapper.Select(q => $"{q.Key}={Uri.EscapeDataString(q.Value)}")));
            }

            return new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);
        }
    }
}
