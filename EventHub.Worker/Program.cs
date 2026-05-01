
using AutoMapper;
using EventHub.Application.Consumer;
using EventHub.Application.Consumers;
using EventHub.Application.Hubs;
using EventHub.Application.Mappings;
using MassTransit;
using TravelSwipe.Shared.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});



// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<LatestImagesForActivitiesConsumer>();
    x.AddConsumer<ActivitiesByLocationConsumer>();

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

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHub<MessagingHub>("/hub");

app.Run();