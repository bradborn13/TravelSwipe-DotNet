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
using Activities.Application.Consumer;
using Activities.Application.Consumers;
using TravelSwipe.Shared.Contracts;
using TravelSwipe.Shared.Messaging;

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

// RabbitMQ
builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddRabbitMqConsumer<FoundActivitiesForLocationEvent, FoundActivityForLocationConsumer>(MessagingTopology.Queues.ActivityFound);
builder.Services.AddRabbitMqConsumer<LatestImagesForActivitiesEvent, LatestImagesForActivitiesConsumer>(MessagingTopology.Queues.ActivityImages);

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