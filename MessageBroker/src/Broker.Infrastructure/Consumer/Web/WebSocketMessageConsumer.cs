using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;

using System.Reactive.Subjects;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;
using Broker.Application.Abstractions.Consumer;
using System.Reactive.Linq;

namespace Broker.Infrastructure.Consumer.Web
{
    public class WebSocketMessageConsumer : IMessageConsumer
    {
        public string ConsumerId { get; }
        public string Topic { get; }
        public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();
        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private readonly WebSocket _socket;

        public WebSocketMessageConsumer(string consumerId, string topic, WebSocket socket)
        {
            ConsumerId = consumerId;
            Topic = topic;
            _socket = socket;
        }

        public Task<Broker.Context.Response.Response> ConsumeAsync(Message message, CancellationToken cancellation)
        {
            // TODO: Implement message send via WebSocket
            return Task.FromResult(new Broker.Context.Response.Response { Success = true });
        }

        public void Dispose()
        {
            _ackSubject.OnCompleted();
            _ackSubject.Dispose();
            _socket.Dispose();
        }
    }
}
