// Broker.Infrastructure/Consumer/Sockets/SocketMessageConsumer.cs
using Broker.Application.Abstractions.Consumer;
using Broker.Context.Response;
using Broker.Domain.Entites.Consumer;
using Broker.Domain.Entites.Messages;
using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Infrastructure.Consumer.Sockets
{
    public class SocketMessageConsumer : IMessageConsumer
    {
        public string ConsumerId { get; }
        public string Topic { get; }
        public IObservable<MessageAcknowledgment> Acks => _ackSubject;
        private readonly Socket _socket;
        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private bool _disposed;

        public SocketMessageConsumer(string consumerId, string topic, Socket socket)
        {
            ConsumerId = consumerId;
            Topic = topic;
            _socket = socket;
        }

        public async Task<Response> ConsumeAsync(Message message, CancellationToken cancellation)
        {
            try
            {
                var data = System.Text.Json.JsonSerializer.Serialize(message);
                var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                await _socket.SendAsync(bytes, SocketFlags.None, cancellation);

                // Simulate ACK for demo (replace with real ACK logic)
                _ackSubject.OnNext(new MessageAcknowledgment
                {
                    MessageId = message.Id,
                    Type = AckType.Ack,
                    Reason = null
                });

                return new Response { Success = true, Message = "Sent" };
            }
            catch (Exception ex)
            {
                return new Response { Success = false, Message = ex.Message };
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _ackSubject.OnCompleted();
            _ackSubject.Dispose();
            _socket?.Dispose();
        }
    }
}