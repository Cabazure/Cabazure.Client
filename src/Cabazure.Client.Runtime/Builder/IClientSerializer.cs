namespace Cabazure.Client.Builder
{
    public interface IClientSerializer
    {
        T? Deserialize<T>(string clientName, string json);
        string Serialize(string clientName, object value);
    }
}
