namespace Cabazure.Client.Authentication
{
    public class DateTimeProvider
        : IDateTimeProvider
    {
        public DateTimeOffset GetDateTime()
            => DateTimeOffset.UtcNow;
    }
}
