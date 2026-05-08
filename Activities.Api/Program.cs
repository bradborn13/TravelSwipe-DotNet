using Activitie.Infrastructure.Repositories;
using Activities.Api.Metrics;
using Activities.Application.Services.Activities;
using Activities.Core.Features.Activities;
using Activities.Core.Features.Cities;
using Activities.Core.Features.Countries;
using Activities.Infrastructure.Data;
using Activities.Infrastructure.Repositories;
using Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
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
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ActivityDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

// MongoDB
builder.Services.AddSingleton<MongoContext>();
// MassTransit + RabbitMQ
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<FoundActivityForLocationConsumer>();
//    x.AddConsumer<LatestImagesForActivitiesConsumer>();

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
builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<ConnectionFactory>();
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});

// 3. Register the Publishing Channel
builder.Services.AddSingleton<IChannel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    return connection.CreateChannelAsync().GetAwaiter().GetResult();
});

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["ConnectionStrings:Redis"]
        ?? "localhost:6379";
    options.InstanceName = "redis:";
});


// Repositories
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();



// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
// App Services
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddSingleton<ActivityMetrics>();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



//app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthorization();
app.MapControllers();
app.MapMetrics();
//app.MapHub<MessagingHub>("/hub");

app.Run();