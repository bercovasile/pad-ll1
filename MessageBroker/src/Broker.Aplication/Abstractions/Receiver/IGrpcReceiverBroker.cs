

using Grpc.Core;

namespace Broker.Application.Abstractions.Receiver;

public interface IGrpcReceiverBroker
{
	Task<IBrokerReceiver?> AcceptReceiverAsync<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		IServerStreamWriter<TResponse> responseStream,
		string topic,
		CancellationToken cancellation = default);
}
