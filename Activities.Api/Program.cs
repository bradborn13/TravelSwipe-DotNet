using Activitie.Infrastructure.Repositories;
using Activities.Api.Metrics;
using Activities.Application.Consumer;
using Activities.Application.Services.Activities;
using Activities.Core.Features.Activities;
using Activities.Core.Features.Cities;
using Activities.Core.Features.Countries;
using Activities.Infrastructure.Data;
using Activities.Infrastructure.Repositories;
using Application.ExternalServices;
using AutoMapper;
using Infrastructure.Mappings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Prometheus;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapMetrics();

app.Run();