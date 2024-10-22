using System.Net;

namespace Cabazure.Client.Builder
{
    public interface IMessageResponseBuilder
    {
        IMessageResponseBuilder AddSuccessResponse(HttpStatusCode statusCode);

        IMessageResponseBuilder AddSuccessResponse<TResponseContent>(HttpStatusCode statusCode);

        IMessageResponseBuilder AddErrorResponse(HttpStatusCode statusCode);

        IMessageResponseBuilder AddErrorResponse<TResponseContent>(HttpStatusCode statusCode);

        Task<TResult> GetAsync<TResult>(
            Func<EndpointResponse, TResult> factory,
            CancellationToken cancellationToken);

        Task<EndpointResponse> GetAsync(
            CancellationToken cancellationToken);
    }
}
