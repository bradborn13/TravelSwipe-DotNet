using Cities.Application.Consumers;
using Cities.Application.Mappings;
using Cities.Application.Services;
using Cities.Core.Features.Cities;
using Cities.Core.Features.Countries;
using Cities.Infrastructure.Data;
using Cities.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Messaging;

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

// RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRabbitMqConsumer<CityRegisteredEvent, CityRegisteredConsumer>(MessagingTopology.Queues.CityService);

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


app.Run();
