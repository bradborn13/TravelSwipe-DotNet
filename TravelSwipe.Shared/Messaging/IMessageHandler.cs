namespace TravelSwipe.Shared.Messaging
{
    /// <summary>
    /// Handles one message type. Resolved from a fresh DI scope per message, so handlers can
    /// depend on scoped services (DbContext, repositories). Throwing moves the message to the
    /// queue's error queue.
    /// </summary>
    public interface IMessageHandler<in T> where T : class
    {
        Task HandleAsync(T message, CancellationToken cancellationToken);
    }
}
