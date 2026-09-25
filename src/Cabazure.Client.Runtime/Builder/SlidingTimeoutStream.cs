namespace Cabazure.Client.Builder
{
    /// <summary>
    /// Wraps a response content stream so that reading it resets a linked timeout
    /// <see cref="CancellationTokenSource"/>'s deadline, implementing a sliding/idle timeout
    /// instead of a fixed total-duration timeout. This allows large-but-actively-flowing
    /// downloads to proceed indefinitely while still detecting a stalled connection at any
    /// point during the transfer.
    /// </summary>
    /// <remarks>
    /// This stream does not own <paramref name="timeoutCts"/> and never disposes it; disposal
    /// is the responsibility of whoever created it (see <see cref="Cabazure.Client.StreamResponse"/>).
    /// </remarks>
    internal sealed class SlidingTimeoutStream(
        Stream inner,
        CancellationTokenSource timeoutCts,
        TimeSpan timeout)
        : Stream
    {
        public override bool CanRead => inner.CanRead;

        public override bool CanSeek => inner.CanSeek;

        public override bool CanWrite => false;

        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
        }

        public override void Flush() => inner.Flush();

        public override long Seek(long offset, SeekOrigin origin) => inner.Seek(offset, origin);

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None)
                .GetAwaiter()
                .GetResult();

        public override int ReadByte()
        {
            var buffer = new byte[1];
            var read = Read(buffer, 0, 1);
            return read > 0 ? buffer[0] : -1;
        }

#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP2_0 || NETCOREAPP2_1 || NETCOREAPP2_2 || NETCOREAPP3_0 || NETCOREAPP3_1
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var read = await inner
                .ReadAsync(buffer, offset, count, linkedCts.Token)
                .ConfigureAwait(false);

            OnReadCompleted(read);
            return read;
        }
#else
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            var read = await inner
                .ReadAsync(buffer, linkedCts.Token)
                .ConfigureAwait(false);

            OnReadCompleted(read);
            return read;
        }
#endif

        private void OnReadCompleted(int bytesRead)
        {
            if (bytesRead > 0)
            {
                timeoutCts.CancelAfter(timeout);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                inner.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
