namespace Cabazure.Client.Authentication;

public interface IDateTimeProvider
{
    DateTimeOffset GetDateTime();
}