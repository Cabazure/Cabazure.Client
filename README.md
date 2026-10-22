[![GitHub Actions Workflow Status](https://img.shields.io/github/actions/workflow/status/Cabazure/Cabazure.Client/.github%2Fworkflows%2Fci.yml)](https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml)
[![GitHub Release Date](https://img.shields.io/github/release-date/Cabazure/Cabazure.Client)](https://github.com/Cabazure/Cabazure.Client/releases)
[![NuGet Version](https://img.shields.io/nuget/v/Cabazure.Client?color=blue)](https://www.nuget.org/packages/Cabazure.Client)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Cabazure.Client?color=blue)](https://www.nuget.org/stats/packages/Cabazure.Client?groupby=Version)

[![Branch Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_branchcoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml)
[![Line Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_linecoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml)
[![Method Coverage](https://raw.githubusercontent.com/Cabazure/Cabazure.Client/main/.github/coveragereport/badge_methodcoverage.svg?raw=true)](https://github.com/Cabazure/Cabazure.Client/actions/workflows/ci.yml)

# Cabazure.Client

The Cabazure.Client is a library for creating .NET Clients for your AspNetCore APIs, using Source Generators directed by attributes.

The main design choices in the Cabazure.Client are:
* Code is used to describe the API, rather than an api specification
* Contracts are shared via code used by both Client and Server
* Endpoints are represented as interfaces, for easy mocking in unit testing
* Dependencies are limited to AspNetCore defaults, like DependencyInjection and Options
* Azure.Identity is used for Authentication

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
| `Task<PagedResponse<T[]>>`  | Used for paged responses, exposing `x-continuation` and the optional `x-total-item-count` as `TotalCount` |
| `Task<StreamResponse>`      | Used for endpoints returning a raw stream of data (e.g. file downloads) |

The `[Path]` attribute on the `customerId` parameter of the endpoint method, declares that this parameter corresponds to the endpoint path placeholder. Parameters containing data for an endpoint method should have one of the following attributes describing how they are passed to the endpoint: `[Path]`, `[Query]`, `[Header]` or `[Body]`.

Apart from the data parameters, the following parameter types are supported:

| Parameter Type            | Description                                                             |
| ------------------------- | ---------------------------------------------------------------------- |
| `ClientRequestOptions`    | Allowing the caller to specify further request options.                |
| `PagedRequestOptions` | Allowing the caller to specify further request and pagination options. |
| `CancellationToken`       | Allowing the caller to cancel the async call to the endpoint.          |

#### Header parameters and request content

`[Header]` binds a parameter to a header. Content headers are automatically
routed to the request's content rather than its general headers:

```csharp
[Post("/items")]
Task<EndpointResponse> CreateAsync(
    [Header("Content-Type")] string? contentType,
    [Header("Content-Language")] string? language,
    [Body] string payload,
    CancellationToken cancellationToken);
```

An explicit `Content-Type` replaces the builder's default `application/json`;
it does not change JSON body serialization. Null, empty, or whitespace header
values are omitted, preserving the default content type when none is supplied.

Content header bindings require a `[Body]` parameter, regardless of parameter
order or whether the header parameter is nullable. Without one, generator error
`ECL009` identifies the binding and asks for a body or removal of the header.
Header names are recognized case-insensitively, including names supplied through
constants. This applies to content headers such as `Content-Type`,
`Content-Language`, `Content-Length`, and the other standard content headers.

Content-header recognition and missing-body validation happen in the generator,
not in the runtime builder. Generated methods use `WithContentHeader` for content
headers and `WithHeader` for general headers. The runtime builder applies content
headers only when request content exists; otherwise it skips them without creating
content. An explicit body is retained even if its serializer produces an empty
string; requests without a body still have no content.
Custom `IMessageRequestBuilder` implementations must implement `WithContentHeader`
alongside `WithHeader`.

Parameter bindings retain their existing formatting syntax:
`[Header("x-number", "D4")] int number`. The second constructor argument is still
a parameter format string, not a fixed value. Method-level header declarations
are not supported; use a handwritten facade for fixed or conditional values.
Header values must satisfy the runtime HTTP parser for the particular header.

#### Handwritten interface facades

An endpoint interface can expose friendly public methods with default implementations
that call internal, generated transport methods:

```csharp
[ClientEndpoint("CustomerClient")]
public interface IGetCustomerEndpoint
{
    public Task<EndpointResponse<Customer>> ExecuteAsync(
        string customerId,
        bool refresh,
        ClientRequestOptions options,
        CancellationToken cancellationToken)
        => SendAsync(
            customerId,
            refresh ? "no-cache" : null,
            options,
            cancellationToken);

    [Get("/v1/customers/{customerId}")]
    internal Task<EndpointResponse<Customer>> SendAsync(
        [Path("customerId")] string customerId,
        [Header("Cache-Control")] string? cacheControl,
        ClientRequestOptions options,
        CancellationToken cancellationToken);
}
```

Consumers in another assembly call `ExecuteAsync` but cannot call `SendAsync`.
When `refresh` is false, the null header value is omitted. `no-cache` requests
cache revalidation; whether it refreshes application-managed data depends on the server.

The generator ignores methods with block or expression bodies, including any HTTP
attributes on those methods. Only methods without implementations are generated
and must declare a supported HTTP route. Interfaces containing only default methods
also receive an implementation and can be registered normally.

This pattern requires C# 8 or later and a runtime supporting default interface
implementations. Client projects using it cannot target `netstandard2.0` or .NET
Framework; Cabazure.Client's own runtime library remains `netstandard2.0`.

Internal abstract members also restrict implementations in other assemblies:
handwritten implementations generally need friend-assembly access to implement
them. Mocking such interfaces depends on the mocking framework and its access to
internal members. Do not assume a mock executes default method bodies; exercise
the real generated implementation when testing facade behavior.

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

### Custom handlers and per-call context

Cabazure.Client uses named `HttpClient` instances and supports standard
`DelegatingHandler` registration. A client bootstrap can add handlers through its
`IHttpClientBuilder` callback:

```csharp
services.AddTransient<OnBehalfOfHandler>();
services.AddCabazureClient(
    "CustomerClient",
    j => j.PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    b => b
        .ConfigureHttpClient(c =>
            c.BaseAddress = new Uri("https://customer-api.contoso.com"))
        .AddHttpMessageHandler<OnBehalfOfHandler>());
```

Alternatively, an application can extend the named client after calling the
client library's bootstrap:

```csharp
services.AddCustomerClient();
services.AddTransient<OnBehalfOfHandler>();
services.AddHttpClient("CustomerClient")
    .AddHttpMessageHandler<OnBehalfOfHandler>();
```

Use the name declared by `[ClientEndpoint]`. Registrations for the same name
compose; registering a handler for another name does not affect this client.

For per-call context, derive a typed options class and attach metadata to the
individual request. The following example targets modern .NET:

```csharp
public static class CustomerRequestContext
{
    public static readonly HttpRequestOptionsKey<string> OnBehalfOf =
        new("CustomerClient.OnBehalfOf");
}

public class CustomerRequestOptions : ClientRequestOptions
{
    public string? OnBehalfOf { get; set; }

    protected override void ConfigureHttpRequest(HttpRequestMessage request)
    {
        base.ConfigureHttpRequest(request);

        if (OnBehalfOf is { } userId)
        {
            request.Options.Set(CustomerRequestContext.OnBehalfOf, userId);
        }
    }
}

public sealed class OnBehalfOfHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("x-on-behalf-of")
            && request.Options.TryGetValue(CustomerRequestContext.OnBehalfOf, out var userId))
        {
            request.Headers.Add("x-on-behalf-of", userId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
```

Pass `new CustomerRequestOptions { OnBehalfOf = trustedUserId }` to an endpoint
accepting `ClientRequestOptions`, or declare the typed options on the endpoint
to make this capability discoverable. For pagination, derive the equivalent
options from `PagedRequestOptions` and call `base.ConfigureHttpRequest` to
preserve correlation and pagination headers. The same hook and handler pipeline
also apply to streaming endpoints.

`HttpRequestMessage.Options` requires .NET 5 or later. A client targeting
`netstandard2.0` can instead use `request.Properties` with a unique string key in
both the options hook and handler, or conditionally use the APIs for each target.
The metadata must be read using the same key and type with which it was written.

Endpoint-bound headers are applied before the options hook; handlers run
afterward in configured pipeline order. This example treats context as a
fallback and preserves an explicit endpoint header. Options overrides and
handlers can deliberately remove or replace headers when that is their policy.

Obtain trusted context at the application's request boundary and pass it
explicitly; no `IHttpContextAccessor` is required. Do not blindly forward a
caller-supplied identity header or send it to unrelated downstream services.
Create options per call and do not mutate them while requests are in flight.

Factory-created handlers have their own DI scopes, not the incoming request's
scope, and Cabazure endpoint implementations are singletons. Do not capture
per-user state in either or expect a scoped dependency injected into a handler
to identify the current incoming request. See the
[HttpClient factory scope guidance](https://learn.microsoft.com/dotnet/core/extensions/httpclient-factory-troubleshooting#httpclient-doesnt-respect-scoped-lifetime).

### Streaming responses

Endpoints returning `Task<StreamResponse>` expose the response body as a raw `Stream` via `OkContent`, along with the response's `Content-Type` header via `ContentType`. Since the returned `Stream` reads directly from the underlying HTTP response, `StreamResponse` implements `IDisposable` and must be disposed once the caller is done reading the stream, to release the underlying HTTP connection:

```csharp
[ClientEndpoint("CustomerClient")]
public interface IDownloadCustomerAvatarEndpoint
{
    [Get("/v1/customers/{customerId}/avatar")]
    public Task<StreamResponse> ExecuteAsync(
      [Path("customerId")] string customerId,
      ClientRequestOptions options,
      CancellationToken cancellationToken);
}
```

```csharp
using var response = await endpoint.ExecuteAsync(
    customerId,
    new ClientRequestOptions(),
    cancellationToken);

if (response is { OkContent: { } stream })
{
    await stream.CopyToAsync(destination, cancellationToken);
}
```

If `ClientRequestOptions.Timeout` is set, it is applied as a **sliding (idle) timeout** while
reading `OkContent`, rather than a fixed total-duration timeout: the deadline resets every time
a chunk of data is successfully read, instead of being a single timer covering the whole
download. This means a large-but-actively-flowing download is never penalized just for taking
longer than `Timeout` in total, while a connection that stalls at any point - waiting for
headers, waiting for the first byte, or mid-transfer - still gets cancelled once no data has
arrived within a `Timeout` window.
