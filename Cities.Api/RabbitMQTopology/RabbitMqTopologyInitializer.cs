using RabbitMQ.Client;

namespace Cities.Api.RabbitMQTopology
{
    public class RabbitMqTopologyInitializer
    {
        private readonly IConnection _connection;
        private const string ExchangeName = "travelswipe-exchange";

        public RabbitMqTopologyInitializer(IConnection connection)
        {
            _connection = connection;
        }
        private async Task DeclareCountryQueuesAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: "country-service-queue",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueBindAsync(
                queue: "country-service-queue",
                exchange: ExchangeName,
                routingKey: "country.found"
            );
        }

        public async Task Initialize()
        {
            await using var channel = await _connection.CreateChannelAsync();

            await channel.ExchangeDeclareAsync(
                exchange: "travelswipe-exchange",
                type: ExchangeType.Topic,
                durable: true
            );
            await DeclareCountryQueuesAsync(channel);
        }
    }
}
