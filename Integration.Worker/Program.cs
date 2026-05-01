using Infrastructure.Mappings;
using Integration.Application.Consumer;
using Integration.Application.Integrations.Activities;
using Integration.Application.Integrations.Images;
using Integration.Application.Integrations.Location;
using Integration.Core.Features.Activities;
using Integration.Worker;
using MassTransit;
using System.Net.Http.Headers;
using TravelSwipe.Shared;


var builder = Host.CreateDefaultBuilder(args);



builder.ConfigureServices((context, services) =>
{
    services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

    var configuration = context.Configuration;
    services.Configure<SerpApiOptions>(
    configuration.GetSection("SerpApi"));
    services.AddHttpClient<ISerpService, SerpApiService>(client =>
    {
        client.BaseAddress = new Uri("https://serpapi.com/");
    });

    services.AddHttpClient<FourSquareService>(client =>
    {
        client.BaseAddress = new Uri(
                configuration["Foursquare:BaseUrl"]
            );
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("X-Places-Api-Version", "2025-06-17");
        var apiKey = configuration["Foursquare:ApiKey"];
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    });

    services.AddHttpClient<TavilyService>(client =>
    {
        var baseUrl = configuration["Tavily:BaseUrl"];

        client.BaseAddress = new Uri(
                configuration["Tavily:BaseUrl"]!
            );
        client.DefaultRequestHeaders.Add("Accept", "application/json");

        var apiKey = configuration["Tavily:ApiKey"];
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);
    }).AddTypedClient<ITavilyService>((HttpClient httpClient, IServiceProvider serviceProvider) => new TavilyService(
    httpClient,
    serviceProvider.GetRequiredService<ILogger<TavilyService>>()
));


    services.AddHttpClient<NominatimAPIService>(client =>
    {
        client.BaseAddress = new Uri(
            configuration["Nominatim:BaseUrl"]!
        );

        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("TravelSwipe-App");
    })
    .AddTypedClient<INominatimService>((HttpClient httpClient, IServiceProvider serviceProvider) => new NominatimAPIService(
    httpClient,
    serviceProvider.GetRequiredService<IPublishEndpoint>(),
    serviceProvider.GetRequiredService<ILogger<NominatimAPIService>>()
));
    // MassTransit + RabbitMQ
    services.AddMassTransit(x =>
    {
        x.AddConsumer<ScrapeLocationActivitiesConsumer>();
        x.AddConsumer<FetchLatestImagesForLocationConsumer>();
        x.AddConsumer(typeof(GenericFaultConsumer<>));

        x.UsingRabbitMq((IBusRegistrationContext ctx, IRabbitMqBusFactoryConfigurator cfg) =>
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
});


var host = builder.Build();
await host.RunAsync();