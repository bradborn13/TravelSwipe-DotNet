using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;


namespace EventHub.Application.Hubs
{
    public class UserMessage
    {
        public required string Sender { get; set; }
        public required string Content { get; set; }
        public DateTime SentTime { get; set; }
    }
    public class MessagingHub : Hub
    {
        private static readonly ConcurrentQueue<UserMessage> MessageHistory = new ConcurrentQueue<UserMessage>(); private const int MaxHistory = 50;
        public async Task PostMessage(string content)
        {
            var userMessage = new UserMessage
            {
                Sender = Context.ConnectionId,
                Content = content,
                SentTime = DateTime.UtcNow
            };

            MessageHistory.Enqueue(userMessage);

            while (MessageHistory.Count > MaxHistory)
            {
                MessageHistory.TryDequeue(out _);
            }

            await Clients.All.SendAsync("ReceiveMessage", userMessage);
        }
        public async Task RetrieveMessageHistory() =>
            await Clients.Caller.SendAsync("MessageHistory", MessageHistory.ToList());
    }
}
