using Api;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

EnvironmentConfiguration.DefineInitConfiguration(builder.Configuration).Wait();

var rabbitManager = new RabbitManager(EnvironmentConfiguration.RabbitMqConnectionString);
var consumerChannel = rabbitManager.CreateChannel();
rabbitManager.ConsumerAsync("dynamic-configuration", consumerChannel, async (message) =>
{
    var payload = JsonSerializer.Deserialize<EnvironmentConfiguration.Payload>(message);
    EnvironmentConfiguration.TestDynamicConfiguration = payload.TestDynamicConfiguration;
    EnvironmentConfiguration.SystemDate = payload.SystemDate;
}).Wait();

#region Services

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton(_ => rabbitManager);

#endregion

#region App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/update-configuration", async ([FromBody] EnvironmentConfiguration.Payload request, 
    [FromServices] RabbitManager rabbitManager) =>
{
    var channel = rabbitManager.CreateChannel();
    await rabbitManager.ProducerAsync("dynamic-configuration", "dynamic-configuration", request, channel);
    channel.Dispose();

    return Results.Ok();
})
.WithName("UpdateConfiguration")
.WithOpenApi();

app.MapGet("/get-configuration", () =>
{
    return Results.Ok(new { EnvironmentConfiguration.TestDynamicConfiguration, EnvironmentConfiguration.SystemDate });
})
.WithName("GetConfiguration")
.WithOpenApi();

app.Run();

#endregion
