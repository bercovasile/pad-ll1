using Broker.Application.Abstractions.Receiver;
using Broker.Context.Messages;
using Broker.Context.Response;
using Broker.Infrastructure.Receiver.grpc;
using Grpc.Core;
using Broker.Grpc;

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

		// Delegate handling to the message handler which uses the receiver pipeline
		await _handler.HandleAsync(requestStream, responseStream, topic, context.CancellationToken);
	}
}