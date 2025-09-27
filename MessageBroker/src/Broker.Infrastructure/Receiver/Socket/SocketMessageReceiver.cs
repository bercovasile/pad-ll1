

namespace Broker.Infrastructure.Receiver.Socket;

using Broker.Application.Abstractions;
using Broker.Application.Abstractions.Receiver;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

public class SocketMessageReceiver : IBrokerReceiver
{
	private Socket? _socket;
	public ITopicContext Context { get; private set; }

	public void SetSocket(Socket socket)
	{
		_socket = socket;
	}

	public void SetTopic(ITopicContext topicProvider)
	{
		Context = topicProvider ?? throw new ArgumentNullException(nameof(topicProvider));
	}

	public async Task<T?> ReceiveAsync<T>(CancellationToken cancellation) where T : new()
	{
		if (_socket == null)
			return default;

		var buffer = new byte[4096];
		int received;

		try
		{
			received = await _socket.ReceiveAsync(buffer, SocketFlags.None, cancellation);
			if (received == 0)
				return default; 

			var json = Encoding.UTF8.GetString(buffer, 0, received);
			return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Socket receive error: {ex.Message}");
			return default;
		}
	}

	public async IAsyncEnumerable<T?> ReceiveAsyncEnumerable<T>([EnumeratorCancellation] CancellationToken cancellation = default) where T : new()
	{
		while (!cancellation.IsCancellationRequested && _socket != null && _socket.Connected)
		{
			var msg = await ReceiveAsync<T>(cancellation);
			if (msg != null)
				yield return msg;
		}
	}

	public Task AckAsync<T>(T message, CancellationToken cancellation) => Task.CompletedTask;
	public Task NackAsync<T>(T message, CancellationToken cancellation) => Task.CompletedTask;
}
