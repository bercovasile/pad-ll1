using Broker.Application.Abstractions.Receiver;
using Broker.Context.Messages;
using Broker.Context.Response;
using Broker.Infrastructure.Receiver.grpc;
using Grpc.Core;

namespace Broker.Presentation.Services.gRPC.Handlers;

public class GrpcReceiverServerService : BrokerReceiver.BrokerReceiverBase
{
	private readonly GrpcReceiverMessageHandler _handler;

	public GrpcReceiverServerService(GrpcReceiverMessageHandler handler)
	{
		_handler = handler;
	}

	/// <summary>
	/// Bidirectional streaming method
	/// </summary>
	public override async Task StreamMessages(
	IAsyncStreamReader<MessageRequest> requestStream,
	IServerStreamWriter<Response> responseStream,
	ServerCallContext context)
	{
		string topic = "default";

		// Extrage topicul din primul mesaj
		if (await requestStream.MoveNext(context.CancellationToken))
		{
			var firstMessage = requestStream.Current;
			if (!string.IsNullOrWhiteSpace(firstMessage.TopicName))
			{
				topic = firstMessage.TopicName;
			}

			// Creează receiver și procesează primul mesaj
			var receiver = new GrpcMessageReceiver<MessageRequest, Response>(
				requestStream,
				responseStream,
				topic
			);

			// Trimite primul mesaj către pipeline
			await receiver.ProcessMessageAsync(firstMessage, context.CancellationToken);

			// Continuă cu restul mesajelor din stream
			await foreach (var msg in receiver.ReceiveAsyncEnumerable<MessageRequest>(context.CancellationToken))
			{
				await receiver.ProcessMessageAsync(msg, context.CancellationToken);
			}
		}
	}
}