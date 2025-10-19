

using Broker.Application.Abstractions.Consumer;
using Broker.Context.Messages;
using Broker.Context.Response;
using Broker.Domain.Entites.Consumer;
using Broker.Domain.Entites.Messages;
using Broker.Infrastructure.Protos.Notifier;
using Grpc.Core;
using Grpc.Net.Client;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Broker.Infrastructure.Consumer.Grpc;

public class GrpcMessageConsumer : IMessageConsumer
{
        public string ConsumerId { get; }
        public string Topic { get; }
        public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();

        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private readonly GrpcChannel _channel;
	private CancellationTokenSource _cts = new();
        private bool _disposed = false;

        public GrpcMessageConsumer(string consumerId,
            string topic,
		string clientAddress,
		CancellationToken? parentCancellation = null)
        {
            ConsumerId = consumerId;
            Topic = topic;
		_channel = GrpcChannel.ForAddress(clientAddress);
		if (parentCancellation != null)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(parentCancellation.Value);
            }
        }

	public async Task<Response> ConsumeAsync(Message message, CancellationToken cancellation)
	{
		if (_disposed)
			return new Response { Success = false, Message = "Consumer disposed" };

		try
		{
			var client = new BrokerConsumerNotifier.BrokerConsumerNotifierClient(_channel);
			using var call = client.Consume(cancellationToken: cancellation);

			await call.RequestStream.WriteAsync(ToGrpcMessage(message));
			await call.RequestStream.CompleteAsync();

			await foreach (var response in call.ResponseStream.ReadAllAsync(cancellation))
			{
				return new Response<object>
				{
					Success = response.Success,
					Message = response.Message,
					Data = response.Data

				};
			}


			return new Response { Success = true, Message = "No response received" };
		}
		catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled || rpcEx.StatusCode == StatusCode.Unavailable)
		{
			return new Response { Success = false, Message = $"gRPC stream closed: {rpcEx.Message}" };
		}
		catch (Exception ex)
		{
			return new Response { Success = false, Message = ex.Message };
		}
	}

	public static GrpcMessage ToGrpcMessage( Message message)
	{
		var grpc = new GrpcMessage
		{
			Id = message.Id.ToString(),
			Key = message.Key,
			Value = message.Value,
			Priority = message.Priority,

		};

		if (message.Headers?.Count > 0)
			grpc.Headers.Add(message.Headers);

		return grpc;
	}

	public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _cts.Cancel(); } catch { }
            _ackSubject.Dispose();
        }
}

