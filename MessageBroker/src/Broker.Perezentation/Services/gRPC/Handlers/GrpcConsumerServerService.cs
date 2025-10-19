
using Broker.Application.Abstractions;
using Broker.Infrastructure.Services;
using Broker.Presentation.Protos.Consumer;
using Grpc.Core;


namespace Broker.Presentation.Services.gRPC.Handlers;

public class GrpcConsumerServerService : BrokerConsumer.BrokerConsumerBase
{
	private readonly ILogger<GrpcConsumerServerService> _logger;
	private readonly BrokerConnection _brokerConnection;
	private readonly IBaseTopicProvider _baseTopicProvide;
	public GrpcConsumerServerService(BrokerConnection brokerConnection, IBaseTopicProvider baseTopicProvide, ILogger<GrpcConsumerServerService> logger)
	{
		_brokerConnection = brokerConnection;
		_baseTopicProvide = baseTopicProvide;
		_logger = logger;
	}

	public async override Task Subscribe(IAsyncStreamReader<SubscribeRequest> requestStream, IServerStreamWriter<Response> responseStream, ServerCallContext context)
	{
		try
		{
			if (await requestStream.MoveNext(context.CancellationToken))
			{
				_logger.LogInformation($"Received: {requestStream.Current.Topic} = {requestStream.Current.Address}");

				var topic = await _baseTopicProvide.GetTopicAsync(requestStream.Current.Topic ?? "default", context.CancellationToken);
				await _brokerConnection.AcceptGrpcConsumerAsync(topic.Id.ToString(), requestStream.Current.Address, context.CancellationToken);

				await responseStream.WriteAsync(new Response
				{
					Success = true
				});
			}
		}
		catch (RpcException rpcEx)
		{
			_logger.LogError(rpcEx, "gRPC error in Subscribe method");
			throw;

		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error in Subscribe method");
		}


	}
}
