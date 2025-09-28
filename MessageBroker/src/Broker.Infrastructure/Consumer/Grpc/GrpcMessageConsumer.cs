using System;
using System.Reactive.Subjects;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;
using Broker.Application.Abstractions.Consumer;
using System.Reactive.Linq;
using Grpc.Core;

namespace Broker.Infrastructure.Consumer.Grpc
{
    public class GrpcMessageConsumer : IMessageConsumer
    {
        public string ConsumerId { get; }
        public string Topic { get; }
        public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();
        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private readonly IAsyncStreamReader<object> _clientStream;
        private readonly IServerStreamWriter<object> _serverStream;

        public GrpcMessageConsumer(string consumerId, string topic, IAsyncStreamReader<object> clientStream, IServerStreamWriter<object> serverStream)
        {
            ConsumerId = consumerId;
            Topic = topic;
            _clientStream = clientStream;
            _serverStream = serverStream;
        }

        public Task<Broker.Context.Response.Response> ConsumeAsync(Message message, CancellationToken cancellation)
        {
            // TODO: Implement message send via gRPC
            return Task.FromResult(new Broker.Context.Response.Response { Success = true });
        }

        public void Dispose()
        {
            _ackSubject.OnCompleted();
            _ackSubject.Dispose();
        }
    }
}
