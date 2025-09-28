using System;
using System.Collections.Concurrent;
using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Consumer;

namespace Broker.Infrastructure.Consumer
{
    public class ConsumerManager : IConsumerManager
    {
        private readonly ConcurrentDictionary<string, HashSet<IMessageConsumer>> _topics = new();
        public void Register(IMessageConsumer consumer)
        {
            var set = _topics.GetOrAdd(consumer.Topic, _ => new HashSet<IMessageConsumer>());
            lock (set) { set.Add(consumer); }
        }
        public void Unregister(IMessageConsumer consumer)
        {
            if (_topics.TryGetValue(consumer.Topic, out var set))
            {
                lock (set) { set.Remove(consumer); }
            }
        }
        public IEnumerable<IMessageConsumer> GetConsumers(string topic)
        {
            if (_topics.TryGetValue(topic, out var set))
            {
                lock (set) { return set.ToList(); }
            }
            return Enumerable.Empty<IMessageConsumer>();
        }
    }
}
