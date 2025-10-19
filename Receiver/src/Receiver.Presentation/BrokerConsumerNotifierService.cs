using Broker.Infrastructure.Protos.Notifier;
using Broker.Presentation.Protos.Consumer;
using Grpc.Core;
using System.Text.Json;

public class NotificationReceiverService : BrokerConsumerNotifier.BrokerConsumerNotifierBase
{
	public override async Task Consume(
	IAsyncStreamReader<GrpcMessage> requestStream,
	IServerStreamWriter<GrpcResponse> responseStream,
	ServerCallContext context)
	{
		try
		{
			await foreach (var msg in requestStream.ReadAllAsync(context.CancellationToken))
			{
				Console.WriteLine($"[Client Server] Received message: {msg.Key} -> {msg.Value} -> {JsonSerializer.Serialize(msg)}");

				await responseStream.WriteAsync(new GrpcResponse
				{
					Success = true,
					Message = $"Received message {msg.Key}",
					Data = msg.Value
				});
			}
		}
		catch (IOException ex)
		{
			Console.WriteLine($"[Warning] Client disconnected unexpectedly: {ex.Message}");
		}
		catch (RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
		{
			Console.WriteLine("[Info] Client cancelled the stream.");
		}
	}


}
