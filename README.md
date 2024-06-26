# Cabazure.Client
The Cabazure.Client is a library for creating .NET Clients for your AspNetCore REST APIs, using Source Generators directed by attributes.

The main design choices in the Cabazure.Client are:
* Code is used to describe the API, rather than an api specification
* Contracts are shared via library used by both Client and Server
* Endpoints are represented as interfaces, for easy mocking in unit testing
* Dependency Injection is used for registering endpoints
* Azure.Identity is used for Authentication

## Getting started

It is recommended to keep your contract data types in a separate library, for easy sharing between your AspNetCore project and your Client project.

Ensure your Client project has a reference to the contract data types used by your service and the Cabazure.Client package.

### Adding an endpoint

Endpoints are added by creating interfaces, decorated with attributes like this:

```csharp
[ClientEndpoint("CustomerClient")]
public interface IGetCustomerEndpoint
{
    [Get("/v1/customers/{customerId}")]
    public Task<EndpointResponse<Customer>> ExecuteAsync(
      [Path("customerId")] string customerId,
      ClientRequestOptions options,
      CancellationToken cancellationToken);
}
```

The `[ClientEndpoint]` attribute declares that this is an endpoint, that should have the implementation generated. The client name (`"CustomerClient"`), is used for identifying which `HttpClient` instance name this endpoint should use.

The `[Get]` attribute declares that the interface method is targeting a GET endpoint on the specified path. The path can have place holders like `{customerId}` which can be referenced by one of the method parameters. The following HTTP methods are supported by corresponding attributes: `[Get]`, `[Post]`, `[Put]` or `[Delete]`.

The return type of `Task<EndpointResponse<Customer>>`, is a wrapper for the actual contract data type `Customer`. The following return type wrappers are supported:

| Type                        | Description                              |
| --------------------------- | ----------------------------------------- |
| `Task<EndpointResponse>`    | Used when there is no response content    |
| `Task<EndpointResponse<T>>` | Used for endpoints with a response object |
| `Task<PagedResponse<T[]>>`  | Used for endpoints with a paged response  |

The `[Path]` attribute on the `customerId` parameter of the endpoint method, declares that this parameter corresponds to the endpoint path placeholder. Parameters containing data for an endpoint method should have one of the following attributes describing how they are passed to the endpoint: `[Path]`, `[Query]`, `[Header]` or `[Body]`.

Apart from the data parameters, the following parameter types are supported:

| Parameter Type            | Description                                                             |
| ------------------------- | ---------------------------------------------------------------------- |
| `ClientRequestOptions`    | Allowing the caller to specify further request options.                |
| `ClientPaginationOptions` | Allowing the caller to specify further request and pagination options. |
| `CancellationToken`       | Allowing the caller to cancel the async call to the endpoint.          |

### Adding the bootstrap

To register relevant dependencies for the Client, the generated `AddCabazureClient` extension method on `IServiceCollection` needs to be called. This will register all the endpoints for the specified `HttpClient` instance as singletons in the `IServiceCollection`.

To make it easy for the user of the Client, it is recommended to do this in a bootstrap method like this:

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCustomerClient(
        this IServiceCollection services)
        => services.AddCabazureClient(
            "CustomerClient",
            b => b
              .SetBaseAddress(new Uri("https://customer-api.contoso.com"))
              .AddAuthentication(
                scope: "app://contoso.com/customer-api/.default",
                credential: new DefaultAzureCredential()),
            j => j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
}
```

The call to `AddCabazureClient` needs the following:
* The name of the `HttpClient` instance (matching the one specified on the endpoints)
* Configuration of the `HttpClient` (using an `IHttpClientBuilder`)
* Configuration of the `JsonSerializerOptions` used for serialization of the contracts

### Using the client

To use the Client library, the bootstrap method should be called during composition of the Hosting app, like this:

```csharp
builder.Services.AddCustomerClient();
```

An endpoint can be constructor injected by adding the endpoint interface as a constructor parameter, like this:

```csharp
public class CustomerNameProvider(
    IGetCustomerEndpoint endpoint)
{
    public async Task<string?> GetCustomerNameAsync(
        string customerId,
        CancellationToken cancellationToken)
        => await endpoint.ExecuteAsync(
            customerId,
            new ClientRequestOptions(),
            cancellationToken)
        switch
        {
            { OkContent: { } c } => c.Name,
            _ => null,
        };
}
```
