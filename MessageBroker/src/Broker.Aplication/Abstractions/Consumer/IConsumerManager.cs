using System.Collections.Generic;

namespace Broker.Application.Abstractions.Consumer
{
    public interface IConsumerManager
    {
        void Register(IMessageConsumer consumer);
        void Unregister(IMessageConsumer consumer);
        IEnumerable<IMessageConsumer> GetConsumers(string topic);
    }
}
