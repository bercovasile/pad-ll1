using System;
using System.Threading;
using System.Threading.Tasks;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;

namespace Broker.Application.Abstractions.Consumer
{
    public interface IMessageConsumer : IDisposable
    {
        string ConsumerId { get; }
        string Topic { get; }
        IObservable<MessageAcknowledgment> Acks { get; }
        Task<Broker.Context.Response.Response> ConsumeAsync(Message message, CancellationToken cancellation);
    }
}
