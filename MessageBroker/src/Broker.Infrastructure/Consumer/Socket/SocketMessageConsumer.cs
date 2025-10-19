// Broker.Infrastructure/Consumer/Sockets/SocketMessageConsumer.cs
using Broker.Application.Abstractions.Consumer;
using Broker.Context.Response;
using Broker.Domain.Entites.Consumer;
using Broker.Domain.Entites.Messages;
using System;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;
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
		private JsonSerializerOptions _options = new JsonSerializerOptions
		{
			ReferenceHandler = ReferenceHandler.Preserve, // handles cycles
			WriteIndented = true                          // optional, for readable JSON
		};
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
				var data = JsonSerializer.Serialize(message, _options);
				var bytes = System.Text.Encoding.UTF8.GetBytes(data);
                await _socket.SendAsync(bytes, SocketFlags.None, cancellation);

                
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