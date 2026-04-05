
using AutoMapper;
using EventHub.Application.Consumer;
using EventHub.Application.Hubs;
using EventHub.Application.Mappings;
using MassTransit;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());



// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<FoundActivityForLocationConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.UsePrometheusMetrics();

        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "secretpassword");
        });

        cfg.ConfigureEndpoints(ctx);
    });
});
//services.AddSingleton<HubConnection>(sp =>
//{
//    var config = sp.GetRequiredService<IConfiguration>();
//    var hubUrl = config["EventHub:Url"] ?? "http://localhost:5088";

//    var connection = new HubConnectionBuilder()
//        .WithUrl($"{hubUrl}/hub")
//        .WithAutomaticReconnect()
//        .Build();

//    return connection;
//});


var app = builder.Build();
app.MapHub<MessagingHub>("/hub");
app.MapControllers();
await app.RunAsync();