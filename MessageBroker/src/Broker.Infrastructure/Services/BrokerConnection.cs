using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Consumer;
using Broker.Domain.Entites.Dispatcher;
using Broker.Domain.Entites.Messages;
using Broker.Infrastructure.Consumer.Grpc;
using Broker.Infrastructure.Consumer.Sockets;
using Broker.Infrastructure.Consumer.Web;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;

namespace Broker.Infrastructure.Services
{
    public class BrokerConnection : IDisposable
    {
        private readonly ConcurrentDictionary<string, HashSet<IMessageConsumer>> _topicConsumers = new();
        private readonly Subject<Message> _messageSubject = new();
        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private readonly CancellationTokenSource _cts = new();
	

	   public IObservable<Message> Messages => _messageSubject.AsObservable();
        public IObservable<MessageAcknowledgment> Acknowledgments => _ackSubject.AsObservable();

        public void RegisterConsumer(string topic, IMessageConsumer consumer)
        {
            var set = _topicConsumers.GetOrAdd(topic, _ => new HashSet<IMessageConsumer>());
            lock (set)
            {
                set.Add(consumer);
            }
        }

        public void UnregisterConsumer(string topic, IMessageConsumer consumer)
        {
            if (_topicConsumers.TryGetValue(topic, out var set))
            {
                lock (set)
                {
                    set.Remove(consumer);
                }
            }
        }

        public IEnumerable<IMessageConsumer> GetConsumers(string topic)
        {
            if (_topicConsumers.TryGetValue(topic, out var set))
            {
                lock (set)
                {
                    return set.ToList();
                }
            }
            return Enumerable.Empty<IMessageConsumer>();
        }
        public async Task<IMessageConsumer?> AcceptWebSocketConsumerAsync(HttpContext context, string topic, CancellationToken cancellation = default)
        {
            if (!context.WebSockets.IsWebSocketRequest)
                return null;

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            var consumerId = Guid.NewGuid().ToString();
            var consumer = new WebSocketMessageConsumer(consumerId, topic, socket);
            RegisterConsumer(topic, consumer);

            // Background task to unregister on socket close
            _ = Task.Run(async () =>
            {
                while (socket.State == WebSocketState.Open && !cancellation.IsCancellationRequested)
                {
                    await Task.Delay(500, cancellation).ContinueWith(_ => { });
                }
                UnregisterConsumer(topic, consumer);
                consumer.Dispose();
            }, cancellation);

            // Subscribe to ACKs from this consumer
            consumer.Acks.Subscribe(_ackSubject);
            return consumer;
        }

        // Accepts a gRPC consumer and registers it
        public async Task<IMessageConsumer> AcceptGrpcConsumerAsync(string topic, string address , CancellationToken cancellation = default)
        {
            var consumerId = Guid.NewGuid().ToString();
            var consumer = new GrpcMessageConsumer(consumerId, topic, address);
            RegisterConsumer(topic, consumer);

            // Subscribe to ACKs from this consumer
            consumer.Acks.Subscribe(_ackSubject);
            return consumer;
        }

		
		public async Task<IMessageConsumer?> AcceptSocketConsumerAsync(Socket socket, string topic, CancellationToken cancellation = default)
		{
			if (socket == null || !socket.Connected)
				return null;

			var consumerId = Guid.NewGuid().ToString();
			var consumer = new SocketMessageConsumer(consumerId, topic, socket);
			RegisterConsumer(topic, consumer);

			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
			_ = Task.Run(async () =>
			{
				var buffer = new byte[1];
				try
				{
					while (!cancellation.IsCancellationRequested)
					{
						int received = 0;
						try
						{
							received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellation);
						}
						catch
						{
							break; 
						}

						if (received == 0)
							break; 
					}
				}
				finally
				{
					
					UnregisterConsumer(topic, consumer);
					consumer.Dispose(); ///<- comentat pentru a nu forța închiderea socket-ului
				}
			}, cancellation);

			consumer.Acks.Subscribe(_ackSubject);

			return consumer;
		}


        public void Dispose()
        {
            try
            {
                _cts.Cancel();
                _messageSubject.OnCompleted();
                _ackSubject.OnCompleted();
                _messageSubject.Dispose();
                _ackSubject.Dispose();
                _cts.Dispose();
                foreach (var consumers in _topicConsumers.Values)
                {
                    foreach (var consumer in consumers)
                    {
                        consumer.Dispose();
                    }
                }
            }
            catch { }
        }
    }
}