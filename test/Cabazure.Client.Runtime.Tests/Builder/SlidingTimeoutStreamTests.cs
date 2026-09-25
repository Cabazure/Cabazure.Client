using Cabazure.Client.Builder;

namespace Cabazure.Client.Runtime.Tests.Builder;

public class SlidingTimeoutStreamTests
{
    [Fact]
    public async Task ReadAsync_Should_Reset_Deadline_On_Each_Successful_Read()
    {
        // Each individual read completes well within the timeout window, but the total
        // duration across all reads exceeds a single window - the sliding timeout must not
        // fire as long as data keeps flowing.
        var timeout = TimeSpan.FromMilliseconds(200);
        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(timeout);
        using var inner = new SlowChunkedStream(
            chunkCount: 4,
            delayBetweenChunks: TimeSpan.FromMilliseconds(120));
        using var sut = new SlidingTimeoutStream(inner, timeoutCts, timeout);

        var buffer = new byte[16];
        var totalRead = 0;
        int read;
        while ((read = await sut.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None)) > 0)
        {
            totalRead += read;
        }

        totalRead.Should().Be(4);
        timeoutCts.IsCancellationRequested.Should().BeFalse();
    }

    [Fact]
    public async Task ReadAsync_Should_Throw_When_Read_Stalls_Past_Timeout()
    {
        var timeout = TimeSpan.FromMilliseconds(100);
        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(timeout);
        using var inner = new SlowChunkedStream([TimeSpan.Zero, TimeSpan.FromMilliseconds(500)]);
        using var sut = new SlidingTimeoutStream(inner, timeoutCts, timeout);

        var buffer = new byte[16];

        // First read succeeds immediately and resets the deadline...
        var firstRead = await sut.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
        firstRead.Should().Be(1);

        // ...but the second read stalls for longer than the timeout window.
        var act = () => sut.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ReadAsync_Should_Honor_Callers_Own_CancellationToken()
    {
        using var timeoutCts = new CancellationTokenSource();
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(30));
        using var callerCts = new CancellationTokenSource();
        using var inner = new SlowChunkedStream(
            chunkCount: 2,
            delayBetweenChunks: TimeSpan.FromMilliseconds(500));
        using var sut = new SlidingTimeoutStream(inner, timeoutCts, TimeSpan.FromSeconds(30));

        var buffer = new byte[16];
        var firstRead = await sut.ReadAsync(buffer, 0, buffer.Length, CancellationToken.None);
        firstRead.Should().Be(1);

        callerCts.CancelAfter(TimeSpan.FromMilliseconds(20));
        var act = () => sut.ReadAsync(buffer, 0, buffer.Length, callerCts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public void Members_Should_Delegate_To_Inner_Stream()
    {
        using var timeoutCts = new CancellationTokenSource();
        using var inner = new MemoryStream([1, 2, 3, 4]);
        using var sut = new SlidingTimeoutStream(inner, timeoutCts, TimeSpan.FromSeconds(30));

        sut.CanRead.Should().Be(inner.CanRead);
        sut.CanSeek.Should().Be(inner.CanSeek);
        sut.CanWrite.Should().BeFalse();
        sut.Length.Should().Be(inner.Length);

        sut.Position = 2;
        sut.Position.Should().Be(2);
        inner.Position.Should().Be(2);

        sut.Seek(0, SeekOrigin.Begin).Should().Be(0);

        var actSetLength = () => sut.SetLength(10);
        actSetLength.Should().Throw<NotSupportedException>();

        var actWrite = () => sut.Write([1], 0, 1);
        actWrite.Should().Throw<NotSupportedException>();
    }

    /// <summary>
    /// A stream that yields single-byte reads with a caller-supplied delay before each one,
    /// simulating a download whose chunks arrive at varying speeds.
    /// </summary>
    private sealed class SlowChunkedStream : Stream
    {
        private readonly Queue<TimeSpan> delays;

        public SlowChunkedStream(int chunkCount, TimeSpan delayBetweenChunks)
            : this(Enumerable.Repeat(delayBetweenChunks, chunkCount))
        {
        }

        public SlowChunkedStream(IEnumerable<TimeSpan> delays)
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
