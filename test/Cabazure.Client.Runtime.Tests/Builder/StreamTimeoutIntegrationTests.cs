using System.Net;
using Cabazure.Client.Builder;

namespace Cabazure.Client.Runtime.Tests.Builder;

/// <summary>
/// End-to-end tests of the sliding/idle timeout, exercising the real
/// <see cref="HttpClient"/>/<see cref="HttpClientExtensions.SendStreamAsync"/>/
/// <see cref="MessageResponseBuilder"/>/<see cref="SlidingTimeoutStream"/> pipeline (rather than
/// substituting any of them), including draining the resulting stream via
/// <see cref="Stream.CopyToAsync(Stream)"/> - a common way consumers read
/// <see cref="StreamResponse.OkContent"/> - to confirm it triggers the sliding timeout the same
/// way direct <see cref="Stream.ReadAsync(byte[], int, int)"/> calls do.
/// </summary>
public class StreamTimeoutIntegrationTests
{
    [Fact]
    public async Task Slow_But_Flowing_Download_Should_Succeed_Despite_Exceeding_One_Timeout_Window()
    {
        var timeout = TimeSpan.FromMilliseconds(150);
        var chunkDelays = Enumerable.Repeat(TimeSpan.FromMilliseconds(50), 6);
        using var content = new SlowStreamContent(chunkDelays);
        using var handler = new StubHttpMessageHandler((request, cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        var options = new ClientRequestOptions { Timeout = timeout };

        var sendResult = await client.SendStreamAsync(request, options, CancellationToken.None);
        var builder = new MessageResponseBuilder(
            sendResult.Response,
            Substitute.For<IClientSerializer>(),
            "ClientName");

        using var streamResponse = await builder
            .AddSuccessResponse(HttpStatusCode.OK)
            .WithStreamTimeout(sendResult.TimeoutCts, sendResult.Timeout)
            .GetStreamAsync(CancellationToken.None);

        // Total transfer time (300ms across 6 chunks) exceeds the 150ms timeout window, but no
        // single gap between chunks does, so draining via CopyToAsync should succeed.
        using var destination = new MemoryStream();
        await streamResponse.OkContent!.CopyToAsync(destination);

        destination.Length.Should().Be(6);
    }

    [Fact]
    public async Task Stalled_Download_Should_Throw_When_A_Gap_Between_Chunks_Exceeds_Timeout()
    {
        var timeout = TimeSpan.FromMilliseconds(75);
        var chunkDelays = new[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(500) };
        using var content = new SlowStreamContent(chunkDelays);
        using var handler = new StubHttpMessageHandler((request, cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = content }));
        using var client = new HttpClient(handler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        var options = new ClientRequestOptions { Timeout = timeout };

        var sendResult = await client.SendStreamAsync(request, options, CancellationToken.None);
        var builder = new MessageResponseBuilder(
            sendResult.Response,
            Substitute.For<IClientSerializer>(),
            "ClientName");

        using var streamResponse = await builder
            .AddSuccessResponse(HttpStatusCode.OK)
            .WithStreamTimeout(sendResult.TimeoutCts, sendResult.Timeout)
            .GetStreamAsync(CancellationToken.None);

        using var destination = new MemoryStream();
        var act = () => streamResponse.OkContent!.CopyToAsync(destination);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    /// <summary>
    /// <see cref="HttpContent"/> that returns a stream yielding single-byte chunks, each
    /// preceded by a caller-supplied delay, simulating a download with per-chunk network
    /// latency. Overrides <see cref="CreateContentReadStreamAsync()"/> directly (rather than
    /// relying on the default <see cref="SerializeToStreamAsync"/>-into-a-buffer behavior) so
    /// the delays are actually observed while the stream is read, not while it's created.
    /// </summary>
    private sealed class SlowStreamContent : HttpContent
    {
        private readonly IReadOnlyCollection<TimeSpan> delays;

        public SlowStreamContent(IEnumerable<TimeSpan> delays)
        {
            this.delays = delays.ToArray();
        }

        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(new LazyChunkedStream(delays));

        protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            => CreateContentReadStreamAsync();

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => throw new NotSupportedException("This test content is only meant to be read, not serialized.");

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }

    /// <summary>
    /// A stream that yields single-byte reads, each preceded by a caller-supplied delay.
    /// </summary>
    private sealed class LazyChunkedStream : Stream
    {
        private readonly Queue<TimeSpan> delays;

        public LazyChunkedStream(IEnumerable<TimeSpan> delays)
        {
            this.delays = new Queue<TimeSpan>(delays);
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (delays.Count == 0)
            {
                return 0;
            }

            var delay = delays.Dequeue();
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }

            buffer[offset] = 1;
            return 1;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
