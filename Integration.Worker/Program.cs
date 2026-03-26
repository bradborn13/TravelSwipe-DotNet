using Infrastructure.Mappings;
using Integration.Application.Consumer;
using Integration.Application.Integrations.Activities;
using Integration.Application.Integrations.Images;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using MassTransit;
using System.Net.Http.Headers;

var builder = Host.CreateDefaultBuilder(args);



builder.ConfigureServices((context, services) =>
{
    services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

    var configuration = context.Configuration;
    services.AddHttpClient<SerpApiService>(client =>
    {
        client.BaseAddress = new Uri("https://serpapi.com/");
    }).AddTypedClient<ISerpService>((httpClient, serviceProvider) => new SerpApiService(
    httpClient,
    serviceProvider.GetRequiredService<IConfiguration>()
)); ;
    services.AddHttpClient<FourSquareService>(client =>
    {
        client.BaseAddress = new Uri(
                configuration["Foursquare:BaseUrl"]!
            );
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("X-Places-Api-Version", "2025-06-17");

        var apiKey = configuration["Foursquare:ApiKey"];
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    });
    // MassTransit + RabbitMQ
    services.AddMassTransit(x =>
    {
        x.AddConsumer<ScrapeLocationActivitiesConsumer>();
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.UsePrometheusMetrics();

            cfg.Host(configuration["RabbitMQ:Host"] ?? "rabbitmq", h =>
            {
                h.Username(configuration["RabbitMQ:Username"] ?? "admin");
                h.Password(configuration["RabbitMQ:Password"] ?? "secretpassword");
            });

            cfg.ConfigureEndpoints(ctx);
        });
    });
    services.AddHttpClient<NominatimAPIService>(client =>
    {
        client.BaseAddress = new Uri(
            configuration["Nominatim:BaseUrl"]!
        );

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TravelSwipe-App");
    })
    .AddTypedClient<INominatimService>((httpClient, serviceProvider) => new NominatimAPIService(
    httpClient,
    serviceProvider.GetRequiredService<IPublishEndpoint>(),
    serviceProvider.GetRequiredService<ILogger<NominatimAPIService>>()
));
});


var host = builder.Build();
await host.RunAsync();