using Broker.Presentation.Protos.Reciver;
using Grpc.Core;

namespace Broker.Presentation.Services.gRPC.Handlers;

public class GrpcReceiverServerService : BrokerReceiver.BrokerReceiverBase
{
    private readonly GrpcReceiverMessageHandler _handler;

    public GrpcReceiverServerService(GrpcReceiverMessageHandler handler)
    {
        _handler = handler;
    }

    public override async Task StreamMessages(
        IAsyncStreamReader<MessageRequest> requestStream,
        IServerStreamWriter<Response> responseStream,
        ServerCallContext context)
    {
        await _handler.HandleAsync(requestStream, responseStream, context.CancellationToken);
    }
}