using AzureRest.Client.Endpoints;
using AzureRest.TestApp.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureOptions<ConfigureAzureRestClientOptions>();
builder.Services.AddAzureRestClient();

var app = builder.Build();

var endpoint = app.Services.GetRequiredService<IListSubscriptionsEndpoint>();

var result = await endpoint.ExecuteAsync();
if (result.OkContent is not { } list)
{
    Console.WriteLine(result.StatusCode);
    return;
}

Console.WriteLine($"Found {list.Count.Value} subscriptions ({list.Count.Type}):");
foreach (var subscription in list.Value)
{
    Console.WriteLine($" - {subscription.DisplayName} ({subscription.SubscriptionId})");
}