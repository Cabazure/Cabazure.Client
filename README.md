<a href="https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml"><img alt="GitHub Actions Workflow Status" src="https://img.shields.io/github/actions/workflow/status/Cabazure/Cabazure.Client/.github%2Fworkflows%2Fci.yml" /></a>
<a href="https://github.com/Cabazure/Cabazure.Client/releases"><img alt="GitHub Release Date" src="https://img.shields.io/github/release-date/Cabazure/Cabazure.Client" /></a>
<a href="https://www.nuget.org/packages/Cabazure.Client"><img alt="NuGet Version" src="https://img.shields.io/nuget/v/Cabazure.Client?color=blue" /></a>
<a href="https://www.nuget.org/packages/Cabazure.Client"><img alt="NuGet Downloads" src="https://img.shields.io/nuget/dt/Cabazure.Client?color=blue" /></a>

<a href="https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml"><img alt="Branch Coverage" src="https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_branchcoverage.svg?raw=true" /></a>
<a href="https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml"><img alt="Line Coverage" src="https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_linecoverage.svg?raw=true" /></a>
<a href="https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml"><img alt="Method Coverage" src="https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_methodcoverage.svg?raw=true" /></a>

# Cabazure.Client

The Cabazure.Client is a library for creating .NET Clients for your AspNetCore APIs, using Source Generators directed by attributes.

The main design choices in the Cabazure.Client are:
* Code is used to describe the API, rather than an api specification
* Contracts are shared via code used by both Client and Server
* Endpoints are represented as interfaces, for easy mocking in unit testing
* Align with AspNetCore defaults like DependencyInjection and Options
* Use Azure.Identity for Authentication

## Getting started

Ensure your Client project has a reference to the Cabazure.Client package and the contract data types used by your service.

### 1. Adding an endpoint

Endpoints are added by creating interfaces decorated with attributes, like this:

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

The `[ClientEndpoint]` attribute declares that this is an endpoint, that should have the implementation generated. The client name (`"CustomerClient"`), is used for identifying which `HttpClient` instance name this endpoint should use. The client name should be unique for this Client, and needs to match the client name used when [adding the boot strap](#2-adding-a-bootstrap).

The `[Get]` attribute declares that the interface method is targeting a GET endpoint on the specified path. The path can have place holders like `{customerId}` which can be referenced by one of the method parameters using the `[Path]` attribute.

The following HTTP methods are supported by corresponding attributes: `[Get]`, `[Post]`, `[Put]` or `[Delete]`.

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

### 2. Adding a bootstrap

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
            j => j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            b => b
              .SetBaseAddress(new Uri("https://customer-api.contoso.com"))
              .AddAuthentication(
                scope: "app://contoso.com/customer-api/.default",
                credential: new DefaultAzureCredential()));
}
```

The call to `AddCabazureClient` needs the following:
* The name of the `HttpClient` instance (matching the one specified on the endpoints)
* Configuration of the `JsonSerializerOptions` used for serialization of the contracts
* Configuration of the `HttpClient` (using an `IHttpClientBuilder`)

#### Alternative: Configuration using options

When creating a client for multiple environments, it can make sense to configure these using the options framework.

This can be done by creating an options class for your client, and implementing one of the following Cabazure.Client interfaces:

| Options Interface | Description |
| ----------------- | ----------- |
| `ICabazureClientOptions` | The base options interface used to describe options that can resolve the BaseAddress of the API.  |
| `ICabazureAuthClientOptions` | Options interface used for clients that needs to authenticate. The interface exposes methods for resolving the scope and `TokenCredentials` to be used. |

For the Customer Client the options could look like this:

```csharp
public class CustomerClientOptions : ICabazureAuthClientOptions
{
    public string? EnvironmentName { get; set; }

    public TokenCredential? Credential { get; set; }

    Uri ICabazureClientOptions.GetBaseAddress()
        => EnvironmentName switch
        {
            "prod" => new Uri("https://customer-api.contoso.com"),
            { } env => new Uri($"https://customer-api.{env}.contoso.com"),
            _ => new Uri("https:localhost:7001"),
        };

    string ICabazureAuthClientOptions.GetScope()
        => string.Concat(
            "app://contoso.com/",
            EnvironmentName ?? "dev",
            "/customer-api/.default");

    TokenCredential ICabazureAuthClientOptions.GetCredential()
        => Credential
        ?? throw new InvalidOperationException(
            "Credential must be set to use the Customer client.");
}
```

With these options in place the bootstrap, can be simplified like this:

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static void AddCustomerClient(
        this IServiceCollection services,
        Action<CustomerClientOptions>? clientOptions = null)
        => services.AddCabazureClient(
            "CustomerClient",
            j => j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            clientOptions);
}
```

This allows the caller to either specify the `CustomerClientOptions` inline, or configure them using the options framework, e.g. by implementing `IConfigureOptions<CustomerOptions>` and then register it with the `IServiceCollection.ConfigureOptions<T>()` extension method.

## 3. Using the client

To use the Client library, the bootstrap method should be called during composition of the Hosting app, like this:

```csharp
builder.Services.AddCustomerClient();
```

If options are used, these can be specified inline, like this:

```csharp
builder.Services.AddCustomerClient(o =>
{
    o.EnvironmentName = "dev";
    o.Credential: new DefaultAzureCredential();
});
```

or they can be configured using an implementation of `IConfigureOptions<CustomerClientOptions>`, like this:

```csharp
public class ConfigureCustomerClientOptions
    : IConfigureOptions<CustomerClientOptions>
{
    public void Configure(CustomerClientOptions options)
    {
        options.EnvironmentName = EnvironmentNames.Development;
        options.Credential = new DefaultAzureCredential();
    }
}
```

which is registered in the `IServiceCollection`, like this:

```csharp
builder.Services.ConfigureOptions<ConfigureCustomerClientOptions>();
```

After the Client is configured the endpoints can be used by adding the endpoint interface to the constructor of the consuming class, like this:

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
