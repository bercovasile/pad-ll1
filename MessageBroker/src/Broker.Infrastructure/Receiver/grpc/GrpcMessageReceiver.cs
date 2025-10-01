
using Broker.Application.Abstractions.Receiver;
using global::Broker.Application.Abstractions;
using global::Broker.Application.Abstractions.Receiver;
using global::Broker.Infrastructure.Core;
using Grpc.Core;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Broker.Infrastructure.Receiver.grpc;

public class GrpcMessageReceiver<TRequest, TResponse> : IBrokerReceiver
{
	private readonly IAsyncStreamReader<TRequest> _requestStream;
	private readonly IServerStreamWriter<TResponse> _responseStream;

	public ITopicContext Context { get; private set; }

	public GrpcMessageReceiver(
		IAsyncStreamReader<TRequest> requestStream,
		IServerStreamWriter<TResponse> responseStream,
		string topic)
	{
		_requestStream = requestStream;
		_responseStream = responseStream;
		Context = new TopicContext(topic);
	}

	public async Task<T?> ReceiveAsync<T>(CancellationToken cancellation) where T : new()
	{
		if (await _requestStream.MoveNext(cancellation))
		{
			var msg = _requestStream.Current;
			try
			{
				var json = JsonSerializer.Serialize(msg);
				return JsonSerializer.Deserialize<T>(json);
			}
			catch (JsonException)
			{
				await SafeSendAsync($"Invalid message format for topic '{Context.ContextTopic}'", cancellation);
				return default;
			}
		}

		return default;
	}

	public async IAsyncEnumerable<T?> ReceiveAsyncEnumerable<T>(
		[EnumeratorCancellation] CancellationToken cancellation = default) where T : new()
	{
		while (await _requestStream.MoveNext(cancellation))
		{
			var msg = _requestStream.Current;
			if (msg == null)
				continue;

			var json = JsonSerializer.Serialize(msg);
			yield return JsonSerializer.Deserialize<T>(json);
		}
	}

	private async Task SafeSendAsync(string message, CancellationToken cancellation)
	{
		var response = (object)message; 
		if (_responseStream != null)
		{
			await _responseStream.WriteAsync((TResponse)response);
		}
	}

	public Task AckAsync<T>(T message, CancellationToken cancellation) => Task.CompletedTask;
	public Task NackAsync<T>(T message, CancellationToken cancellation) => Task.CompletedTask;
}

