using System.Collections.Generic;
using Broker.Application.Abstractions.Consumer;

namespace Broker.Infrastructure.Dispatcher
{
    public class ConsumerGroup
    {
        private readonly List<IMessageConsumer> _consumers = new();
        public void AddConsumer(IMessageConsumer consumer) => _consumers.Add(consumer);
        public void RemoveConsumer(IMessageConsumer consumer) => _consumers.Remove(consumer);
        public IMessageConsumer? GetNextConsumer() => _consumers.FirstOrDefault();
        public IEnumerable<IMessageConsumer> GetAvailableConsumers() => _consumers;
    }
}
