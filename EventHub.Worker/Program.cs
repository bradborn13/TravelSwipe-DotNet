
using EventHub.Application.Consumer;
using EventHub.Application.Consumers;
using EventHub.Application.Hubs;
using EventHub.Application.Mappings;
using EventStore.Client;
using System.Text.Json.Nodes;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Messaging;


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




// RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRabbitMqConsumer<ActivitiesByLocationEvent, ActivitiesByLocationConsumer>(MessagingTopology.Queues.EventHubActivities);
builder.Services.AddRabbitMqConsumer<LatestImagesForActivitiesEvent, LatestImagesForActivitiesConsumer>(MessagingTopology.Queues.EventHubImages);

// Every message on the exchange is appended to EventStore (one stream per event type).
builder.Services.AddRabbitMqConsumer<JsonNode, EventStoreRecorder>(MessagingTopology.Queues.EventHubEventStore);


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

builder.Services
       .AddEventStoreClient(builder.Configuration["EventStore"]);

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHub<MessagingHub>("/hub");
app.Run();