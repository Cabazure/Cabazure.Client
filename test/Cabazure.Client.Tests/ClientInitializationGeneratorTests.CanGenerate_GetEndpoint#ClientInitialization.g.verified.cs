//HintName: ClientInitialization.g.cs
using System.Text.Json;
using Cabazure.Client;

namespace Microsoft.Extensions.DependencyInjection;

internal static partial class ClientInitialization
{
    internal static partial IServiceCollection AddCabazureClient(
        this IServiceCollection services,
        string clientName,
        Action<IHttpClientBuilder> builder,
        Action<JsonSerializerOptions> jsonOptions);
}