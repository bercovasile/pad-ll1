
using System;
using System.Reactive.Subjects;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Consumer;
using Broker.Application.Abstractions.Consumer;
using System.Reactive.Linq;
using Grpc.Core;

namespace Broker.Infrastructure.Consumer.Grpc;

// using directives
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using Grpc.Core;
using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Messages;
using Broker.Context.Response;
using System.Text.Json;
using System.Collections.Generic;

// Adjust namespaces according to generated proto C# namespace:
using Messaging.Grpc;

namespace Broker.Infrastructure.Consumer.Grpc
{
	public class GrpcMessageConsumer : IMessageConsumer, IDisposable
	{
		public string ConsumerId { get; }
		public string Topic { get; }
		public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();

		private readonly Subject<MessageAcknowledgment> _ackSubject = new();
		private readonly IAsyncStreamReader<AckProto> _clientStream;
		private readonly IServerStreamWriter<MessageProto> _serverStream;
		private readonly CancellationTokenSource _cts = new();
		private bool _disposed = false;
		private readonly Task _ackReaderTask;

		public GrpcMessageConsumer(string consumerId, string topic,
			IAsyncStreamReader<AckProto> clientStream,
			IServerStreamWriter<MessageProto> serverStream,
			CancellationToken? parentCancellation = null)
		{
			ConsumerId = consumerId;
			Topic = topic;
			_clientStream = clientStream;
			_serverStream = serverStream;

			// create linked token so stopping parent cancels ack reader
			CancellationToken linked = parentCancellation ?? CancellationToken.None;
			if (parentCancellation != null)
			{
				// If parent provided, create linked CTS
				_cts = CancellationTokenSource.CreateLinkedTokenSource(linked);
			}

			// Start reading ACKs from client in background
			_ackReaderTask = Task.Run(() => ReadAcksLoopAsync(_cts.Token));
		}

		public async Task<Broker.Context.Response.Response> ConsumeAsync(Message message, CancellationToken cancellation)
		{
			if (_disposed) return new Broker.Context.Response.Response { Success = false, Message = "Consumer disposed" };

			try
			{
				// Map your domain Message to MessageProto
				var proto = new MessageProto
				{
					Id = message.Id.ToString(),
					Key = message.Key,
					Value = message.Value,
					Priority = message.Priority,
					Timestamp = message.Timestamp.ToString("o"),
					TopicId = message.TopicId.ToString()
				};

				if (message.Headers != null)
				{
					proto.Headers.Add(message.Headers);
				}

				// write to the response stream (server -> client)
				await _serverStream.WriteAsync(proto).ConfigureAwait(false);

				return new Broker.Context.Response.Response { Success = true, Message = "Sent" };
			}
			catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled || rpcEx.StatusCode == StatusCode.Unavailable)
			{
				return new Broker.Context.Response.Response { Success = false, Message = $"gRPC stream closed: {rpcEx.Message}" };
			}
			catch (Exception ex)
			{
				return new Broker.Context.Response.Response { Success = false, Message = ex.Message };
			}
		}

		private async Task ReadAcksLoopAsync(CancellationToken cancellation)
		{
			try
			{
				while (!cancellation.IsCancellationRequested && await _clientStream.MoveNext(cancellation).ConfigureAwait(false))
				{
					var ack = _clientStream.Current;
					if (ack == null) continue;

					// convert AckProto -> MessageAcknowledgment (your domain ack model)
					var domainAck = new MessageAcknowledgment
					{
						MessageId = Guid.TryParse(ack.MessageId, out var g) ? g : Guid.Empty,
						Type = ack.Type == "Ack" ? AckType.Ack : AckType.Nack,
						Reason = string.IsNullOrWhiteSpace(ack.Reason) ? null : ack.Reason
					};

					_ackSubject.OnNext(domainAck);
				}
			}
			catch (OperationCanceledException) { /* expected on cancel */ }
			catch (RpcException) { /* stream broken */ }
			catch (Exception ex)
			{
				// optionally log
				_ackSubject.OnError(ex);
			}
			finally
			{
				_ackSubject.OnCompleted();
			}
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;
			try
			{
				_cts.Cancel();
			}
			catch { }

			try
			{
				_ackReaderTask?.Wait(TimeSpan.FromSeconds(2));
			}
			catch { }

			_ackSubject.Dispose();
		}
	}
}
