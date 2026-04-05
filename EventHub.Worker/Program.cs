
using AutoMapper;
using EventHub.Application.Consumer;
using EventHub.Application.Mappings;
using MassTransit;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http.Headers;
using TravelSwipe.Shared.Contracts;
var builder = Host.CreateDefaultBuilder(args);



builder.ConfigureServices((context, services) =>
{
    services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

    var configuration = context.Configuration;

    // MassTransit + RabbitMQ
    services.AddMassTransit(x =>
    {
        //x.AddConsumer<ScrapeLocationActivitiesConsumer>();
        x.AddConsumer<FoundActivityForLocationConsumer>();

        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.UsePrometheusMetrics();

            cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "admin");
                h.Password(configuration["RabbitMQ:Password"] ?? "secretpassword");
            });

            cfg.ConfigureEndpoints(ctx);
        });
    });
    services.AddSingleton<HubConnection>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var hubUrl = config["EventHub:Url"] ?? "http://localhost:5088";

        var connection = new HubConnectionBuilder()
            .WithUrl($"{hubUrl}/hub")
            .WithAutomaticReconnect()
            .Build();

        return connection;
    });
});



var host = builder.Build();
await host.RunAsync();