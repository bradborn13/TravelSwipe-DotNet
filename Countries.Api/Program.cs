using Countries.Application.Consumer;
using Countries.Application.Mappings;
using Countries.Application.Services;
using Countries.Core.Features.Countries;
using Countries.Infrastructure.Data;
using Countries.Infrastructure.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using TravelSwipe.Shared.Contracts;
;

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
//PostgreSQL
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CountryDbContext>(options =>
    options.UseNpgsql(connectionStringPostgres));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CountryRegisteredConsumer>();

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
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICountryService, CountryService>();

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
