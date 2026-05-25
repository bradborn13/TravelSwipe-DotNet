
using EventHub.Application.Hubs;
using EventHub.Application.Mappings;
using EventHub.Worker.RabbitMQTopology;
using EventStore.Client;
using RabbitMQ.Client;


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
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<LatestImagesForActivitiesConsumer>();
//    x.AddConsumer<ActivitiesByLocationConsumer>();

//    x.UsingRabbitMq((ctx, cfg) =>
//    {
//        cfg.UsePrometheusMetrics();

//        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
//        {
//            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
//            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "secretpassword");
//        });

//        cfg.ConfigureEndpoints(ctx);
//    });
//});

var factory = new ConnectionFactory
{
    HostName = builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq",
    UserName = builder.Configuration["RabbitMQ:Username"] ?? "admin",
    Password = builder.Configuration["RabbitMQ:Password"] ?? "secretpassword",
};
builder.Services.AddSingleton<IConnection>(sp =>
{
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddSingleton<RabbitMqTopologyInitializer>();


// 3. Register the Publishing Channel
builder.Services.AddSingleton<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().GetAwaiter().GetResult();
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

builder.Services
       .AddEventStoreClient(builder.Configuration["EventStore"]);

builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapHub<MessagingHub>("/hub");
using (var scope = app.Services.CreateScope())
{
    var topology = scope.ServiceProvider
        .GetRequiredService<RabbitMqTopologyInitializer>();

    await topology.Initialize();
}
app.Run();