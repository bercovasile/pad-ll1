using Broker.Presentation.Abstractions;
using System.Text.Json;
using System.Threading.Channels;


namespace Broker.Presentation.gRPC;

//public class GrpcMessageReceiver : IMessageReceiver
//{
//	private readonly Channel<MessageEnvelope> _channel = Channel.CreateUnbounded<MessageEnvelope>();

//	public async Task EnqueueAsync(string payload, CancellationToken cancellation = default)
//	{
//		var envelope = new MessageEnvelope(payload);
//		await _channel.Writer.WriteAsync(envelope, cancellation);
//	}

//	public async Task<T?> ReceiveAsync<T>(CancellationToken cancellation)
//	{
//		var envelope = await _channel.Reader.ReadAsync(cancellation);
//		return JsonSerializer.Deserialize<T>(envelope.Payload);
//	}

//	public Task AckAsync<T>(T message, CancellationToken cancellation)
//		=> Task.CompletedTask;

//	public Task NackAsync<T>(T message, CancellationToken cancellation)
//		=> Task.CompletedTask;
//}

//public record MessageEnvelope(string Payload);
