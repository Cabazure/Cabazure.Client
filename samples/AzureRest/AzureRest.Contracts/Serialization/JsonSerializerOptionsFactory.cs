using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureRest.Contracts.Serialization
{
    public static class JsonSerializerOptionsFactory
    {
        public static JsonSerializerOptions Create()
            => Configure(new JsonSerializerOptions());

        public static JsonSerializerOptions Configure(
            this JsonSerializerOptions options)
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(
                new JsonStringEnumConverter(
                    JsonNamingPolicy.CamelCase));

            return options;
        }
    }
}
