using RabbitMQ.Client;

namespace EventHub.Worker.RabbitMQTopology
{
    public class RabbitMqTopologyInitializer
    {
        private readonly IConnection _connection;
        private const string ExchangeName = "travelswipe-exchange";

        public RabbitMqTopologyInitializer(IConnection connection)
        {
            _connection = connection;
        }
        private async Task DeclareCityQueuesAsync(IChannel channel)
        {
            await channel.QueueDeclareAsync(
                queue: "eventhub-city-queue",
                durable: true,
                exclusive: false,
                autoDelete: false
            );

            await channel.QueueBindAsync(
                queue: "eventhub-city-queue",
                exchange: ExchangeName,
                routingKey: "city.registered"
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
            await DeclareCityQueuesAsync(channel);
        }
    }
}
