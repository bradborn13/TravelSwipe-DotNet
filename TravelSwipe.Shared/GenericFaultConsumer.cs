using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelSwipe.Shared
{
    public class GenericFaultConsumer<T> : IConsumer<Fault<T>>
      where T : class
    {
        private readonly ILogger<GenericFaultConsumer<T>> _logger;

        public GenericFaultConsumer(ILogger<GenericFaultConsumer<T>> logger)
        {
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<Fault<T>> context)
        {
            _logger.LogError(
                "Fault handling {EventType}. Exceptions: {Exceptions}",
                typeof(T).Name,
                string.Join(", ", context.Message.Exceptions.Select(e => e.Message)));

            await Task.CompletedTask;
        }
    }
}
