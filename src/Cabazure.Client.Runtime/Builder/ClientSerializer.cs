using System.Text.Json;
using Microsoft.Extensions.Options;

namespace Cabazure.Client.Builder;

public class ClientSerializer(
    IOptionsMonitor<JsonSerializerOptions> options)
    : IClientSerializer
{
    public string Serialize(string clientName, object value)
        => JsonSerializer.Serialize(value, options.Get(clientName));

    public T? Deserialize<T>(string clientName, string json)
        => JsonSerializer.Deserialize<T>(json, options.Get(clientName));
}
