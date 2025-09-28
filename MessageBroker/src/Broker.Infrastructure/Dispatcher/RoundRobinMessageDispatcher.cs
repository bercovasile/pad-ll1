using System;
using System.Collections.Concurrent;
using Broker.Application.Abstractions.Dispatcher;
using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;
using Broker.Domain.Entites.Dispatcher;

namespace Broker.Infrastructure.Dispatcher
{
    public class RoundRobinMessageDispatcher : IMessageDispatcher
    {
        private readonly IConsumerManager _consumerManager;
        public RoundRobinMessageDispatcher(IConsumerManager consumerManager)
        {
            _consumerManager = consumerManager;
        }
        public async Task<MessageDispatchResult> DispatchAsync(Message message, CancellationToken cancellation)
        {
            var consumers = _consumerManager.GetConsumers(message.TopicId.ToString()).ToList();
            if (!consumers.Any())
                return new MessageDispatchResult { Delivered = false, Reason = "No consumers" };
            var selected = consumers.First();
            var sendResp = await selected.ConsumeAsync(message, cancellation);
            if (!sendResp.Success)
                return new MessageDispatchResult { Delivered = false, Reason = sendResp.Message };
            // TODO: Wait for ACK/NACK
            return new MessageDispatchResult { Delivered = true };
        }
    }
}
