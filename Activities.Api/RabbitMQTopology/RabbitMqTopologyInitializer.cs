using RabbitMQ.Client;

namespace Activities.Api.RabbitMQTopology
{

    public class RabbitMqTopologyInitializer
    {
        private readonly IConnection _connection;
        private const string ExchangeName = "travelswipe-exchange";

        public RabbitMqTopologyInitializer(IConnection connection)
        {
            _connection = connection;
        }
        private async Task DeclareActivityQueuesAsync(IChannel channel)
        {

            await channel.QueueDeclareAsync(
               queue: "activity-scrape-queue",
               durable: true,
               exclusive: false,
               autoDelete: false
           );

            await channel.QueueBindAsync(
                queue: "activity-scrape-queue",
                exchange: ExchangeName,
                routingKey: "scrapeEvents.by.location"
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
            await DeclareActivityQueuesAsync(channel);
        }
    }
}
