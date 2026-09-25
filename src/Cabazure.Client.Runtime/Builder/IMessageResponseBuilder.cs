using System.Net;

namespace Cabazure.Client.Builder
{
    public interface IMessageResponseBuilder
    {
        IMessageResponseBuilder AddSuccessResponse(HttpStatusCode statusCode);

        IMessageResponseBuilder AddSuccessResponse<TResponseContent>(HttpStatusCode statusCode);

        IMessageResponseBuilder AddErrorResponse(HttpStatusCode statusCode);

        IMessageResponseBuilder AddErrorResponse<TResponseContent>(HttpStatusCode statusCode);

        /// <summary>
        /// Configures the sliding/idle timeout to apply while reading a stream response's
        /// body via <see cref="GetStreamAsync"/>. Has no effect on <see cref="GetAsync(CancellationToken)"/>.
        /// </summary>
        /// <param name="timeoutCts">
        /// The linked timeout <see cref="CancellationTokenSource"/> returned by
        /// <see cref="Cabazure.Client.Builder.HttpClientExtensions.SendStreamAsync"/>, or
        /// <see langword="null"/> if no timeout was configured for the request.
        /// </param>
        /// <param name="timeout">The configured timeout, used to reset <paramref name="timeoutCts"/>'s deadline on each read.</param>
        IMessageResponseBuilder WithStreamTimeout(CancellationTokenSource? timeoutCts, TimeSpan? timeout);

        Task<TResult> GetAsync<TResult>(
            Func<EndpointResponse, TResult> factory,
            CancellationToken cancellationToken);

        Task<EndpointResponse> GetAsync(
            CancellationToken cancellationToken);

        Task<StreamResponse> GetStreamAsync(
            CancellationToken cancellationToken);
    }
}
