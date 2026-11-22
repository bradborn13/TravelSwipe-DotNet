namespace TravelSwipe.Shared.Messaging
{
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes <paramref name="message"/> to the shared exchange, using the routing key
        /// registered for <typeparamref name="T"/> in <see cref="MessagingTopology"/>.
        /// </summary>
        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default) where T : class;
    }
}
