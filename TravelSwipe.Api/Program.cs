using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using TravelSwipe.Application.ExternalServices;
using TravelSwipe.Core.Core.Users;
using TravelSwipe.Core.Features.Activities;
using TravelSwipe.Core.Features.Users;
using TravelSwipe.Infrastructure.data;
using TravelSwipe.Infrastructure.Data;
using TravelSwipe.Infrastructure.Mappings;
using TravelSwipe.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
MongoMapping.Configure();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("MongoDb");
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddScoped<IActivityRepository, ActivityRepository>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHttpClient<SerpApiService>(client =>
{
    client.BaseAddress = new Uri("https://serpapi.com/");
});
builder.Services.AddHttpClient<FourSquareService>(client =>
{
    client.BaseAddress = new Uri("https://places-api.foursquare.com");
});
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
