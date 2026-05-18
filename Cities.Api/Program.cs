using Cities.Api.RabbitMQTopology;
using Cities.Application.Consumers;
using Cities.Application.Mappings;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using Cities.Core.Features.Countries;
using Cities.Infrastructure.Data;
using Cities.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

// Register each worker individually
builder.Services.AddHostedService<CityRegisteredConsumer>();
//builder.Services.AddMassTransit(x =>
//{
//    x.AddConsumer<CityRegisteredConsumer>();

//    x.UsingRabbitMq((ctx, cfg) =>
//    {
//        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
//        {
//            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
//            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "secretpassword");
//        });

//        cfg.ConfigureEndpoints(ctx);
//    });
//});
//PostgreSQL
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CityDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["ConnectionStrings:Redis"]
        ?? "localhost:6379";
    options.InstanceName = "redis:";
});

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
// Repositories
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityService, CityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var topology = scope.ServiceProvider
        .GetRequiredService<RabbitMqTopologyInitializer>();

    await topology.Initialize();
}

app.Run();
