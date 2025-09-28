using System;
using System.Reactive.Subjects;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;
using Broker.Application.Abstractions.Consumer;
using System.Reactive.Linq;

namespace Broker.Infrastructure.Consumer.Core
{
    public abstract class BaseMessageConsumer : IMessageConsumer
    {
        public string ConsumerId { get; protected set; } = string.Empty;
        public string Topic { get; protected set; } = string.Empty;
        public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();
        protected readonly Subject<MessageAcknowledgment> _ackSubject = new();
        public abstract Task<Broker.Context.Response.Response> ConsumeAsync(Message message, CancellationToken cancellation);
        public virtual void Dispose()
        {
            _ackSubject.OnCompleted();
            _ackSubject.Dispose();
        }
    }
}
