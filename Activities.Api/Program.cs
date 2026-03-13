using AutoMapper;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using TravelSwipe.Activities.Application.Services.Activities;
using TravelSwipe.Activities.Core.Features.Activities;
using TravelSwipe.Activities.Core.Features.Cities;
using TravelSwipe.Activities.Core.Features.Countries;
using TravelSwipe.Activities.Infrastructure.Data;
using TravelSwipe.Activities.Infrastructure.Repositories;
using TravelSwipe.Application.ExternalServices;
using TravelSwipe.Infrastructure.data;
using TravelSwipe.Infrastructure.Mappings;
using TravelSwipe.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<ActivityDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

// MongoDB
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddHttpClient<SerpApiService>(client =>
{
    client.BaseAddress = new Uri("https://serpapi.com/");
});
builder.Services.AddHttpClient<FourSquareService>(client =>
{
    client.BaseAddress = new Uri(
            builder.Configuration["Foursquare:BaseUrl"]!
        );
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("X-Places-Api-Version", "2025-06-17");

    var apiKey = builder.Configuration["Foursquare:ApiKey"];
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);
});
// MassTransit + RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "admin");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "secretpassword");
        });

        cfg.ConfigureEndpoints(ctx);
    });
});
builder.Services.AddHttpClient<NominatimAPIService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Nominatim:BaseUrl"]!
    );

    client.DefaultRequestHeaders.Add("Accept", "application/json");

    // Nominatim requires a valid user agent
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TravelSwipe-App");
});
// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"]
        ?? "localhost:6379";
    options.InstanceName = "redis:";
});

// Repositories
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();

// External Services
builder.Services.AddHttpClient<FourSquareService>();
builder.Services.AddHttpClient<SerpApiService>();
builder.Services.AddHttpClient<NominatimAPIService>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// App Services
builder.Services.AddScoped<IActivityService, ActivityService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();