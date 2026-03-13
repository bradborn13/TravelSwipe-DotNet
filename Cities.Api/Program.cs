using Cities.Application.Consumers;
using Cities.Application.Mappings;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using Cities.Core.Features.Countries;
using Cities.Infrastructure.Data;
using Cities.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CityDiscoveredConsumer>();  // references Application layer

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "secretpassword");
        });

        cfg.ConfigureEndpoints(ctx);
    });
});
//PostgreSQL
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CityDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"]
        ?? "localhost:6379";
    options.InstanceName = "redis:";
});

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

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

app.UseAuthorization();

app.MapControllers();

app.Run();
