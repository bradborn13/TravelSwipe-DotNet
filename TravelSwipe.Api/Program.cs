using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using TravelSwipe.Application.ExternalServices;
using TravelSwipe.Application.Services;
using TravelSwipe.Application.Services.Activities;
using TravelSwipe.Core;
using TravelSwipe.Core.Core.Users;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Cities;
using TravelSwipe.Core.Features.Countries;
using TravelSwipe.Core.Features.Users;
using TravelSwipe.Infrastructure;
using TravelSwipe.Infrastructure.data;
using TravelSwipe.Infrastructure.Data;
using TravelSwipe.Infrastructure.Mappings;
using TravelSwipe.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
MongoMapping.Configure();

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddScoped<IActivityService, ActivityService>();
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
builder.Services.AddHttpClient<NominatimAPIService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Nominatim:BaseUrl"]!
    );

    client.DefaultRequestHeaders.Add("Accept", "application/json");

    // Nominatim requires a valid user agent
    client.DefaultRequestHeaders.UserAgent.ParseAdd("TravelSwipe-App");
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "redis:";
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
//if (!app.Environment.IsDevelopment())
//{
//    app.UseHttpsRedirection();
//}
app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run();
