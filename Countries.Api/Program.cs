using Countries.Application.Mappings;
using Countries.Core.Features.Countries;
using Countries.Infrastructure.Data;
using Countries.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//PostgreSQL
var connectionStringPostgres = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddDbContext<CountryDbContext>(options =>
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
builder.Services.AddScoped<ICountryRepository, CountryRepository>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
