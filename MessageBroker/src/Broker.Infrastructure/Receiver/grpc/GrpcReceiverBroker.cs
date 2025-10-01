using Broker.Application.Abstractions.Receiver;
using global::Broker.Application.Abstractions.Receiver;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Broker.Infrastructure.Receiver.grpc; 

public class GrpcReceiverBroker : IGrpcReceiverBroker
{
	private readonly IServiceProvider _provider;

	public GrpcReceiverBroker(IServiceProvider provider)
	{
		_provider = provider;
	}

	public Task<IBrokerReceiver?> AcceptReceiverAsync<TRequest, TResponse>(
		IAsyncStreamReader<TRequest> requestStream,
		IServerStreamWriter<TResponse> responseStream,
		string topic,
		CancellationToken cancellation = default)
	{
		var receiver = _provider.GetRequiredService<IBrokerReceiver>();

		if (receiver is GrpcMessageReceiver<TRequest, TResponse> grpcReceiver)
		{
			return Task.FromResult<IBrokerReceiver?>(grpcReceiver);
		}

		// dacă e nevoie, construim unul dinamic
		var dynamicReceiver = new GrpcMessageReceiver<TRequest, TResponse>(requestStream, responseStream, topic);
		return Task.FromResult<IBrokerReceiver?>(dynamicReceiver);
	}
}

