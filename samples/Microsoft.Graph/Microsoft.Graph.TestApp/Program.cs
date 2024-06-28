using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Graph.Client;
using Microsoft.Graph.Client.Endpoints;
using Microsoft.Graph.TestApp.Options;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureOptions<ConfigureGraphClientOptions>();
builder.Services.AddGraphClient();

var app = builder.Build();

var endpoint = app.Services.GetRequiredService<IGetUserByEmail>();

var result = await endpoint.ExecuteAsync("frannis@contoso.com") switch
{
    { OkContent: { } r } => r.DisplayName,
    var r => r.StatusCode.ToString(),
};

Console.WriteLine(result);