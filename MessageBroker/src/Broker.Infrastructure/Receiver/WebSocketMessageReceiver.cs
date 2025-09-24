using Broker.Application.Abstractions;
using Broker.Application.Abstractions.Receiver;

using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Broker.Presentation.Socket.Receiver;

public class WebSocketMessageReceiver : IMessageReceiver
{
	private WebSocket? _socket;

	public ITopicContext  Context { get; private set; }



	public void SetWebSocket(WebSocket socket)
	{
		_socket = socket;
	}

	public void SetTopic(ITopicContext topicProvider)
	{
		Context = topicProvider ?? throw new ArgumentNullException(nameof(topicProvider));
	}

	public async Task<T?> ReceiveAsync<T>(CancellationToken cancellation) where T : new()
	{
		var buffer = new byte[1024 * 4];
		WebSocketReceiveResult result;

		try
		{
			result = await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);

			// If the client is closing, acknowledge and stop
			if (result.MessageType == WebSocketMessageType.Close)
			{
				if (_socket.State == WebSocketState.CloseReceived)
				{
					await _socket.CloseOutputAsync(
						WebSocketCloseStatus.NormalClosure,
						"Closing",
						cancellation
					);
				}
				return default;
			}

			var json = Encoding.UTF8.GetString(buffer, 0, result.Count);

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true
			};

			try
			{
				return JsonSerializer.Deserialize<T>(json, options);
			}
			catch (JsonException)
			{
				var errorMessage = $"Invalid message format for topic '{Context.ContextTopic}'";
				await SafeSendAsync(errorMessage, cancellation);
				return default;
			}
		}
		catch (OperationCanceledException)
		{
			Console.WriteLine("WebSocket receive operation was canceled.");
			return default;
		}
		catch (WebSocketException wse)
		{
			Console.WriteLine($"WebSocket error: {wse.Message}");
			return default;
		}
	}

	/// <summary>
	/// Asynchronous stream of messages until the socket is closed or the token is cancelled.
	/// </summary>
	public async IAsyncEnumerable<T?> ReceiveAsyncEnumerable<T>(
		[EnumeratorCancellation] CancellationToken cancellation = default) where T : new()
	{
		while (!cancellation.IsCancellationRequested &&
			   _socket != null &&
			   _socket.State == WebSocketState.Open)
		{
			T? msg;
			try
			{
				msg = await ReceiveAsync<T>(cancellation);
			}
			catch (WebSocketException)
			{
				yield break; // stop streaming if socket is closed
			}

			if (msg == null)
				continue;

			yield return msg;
		}
	}


	/// <summary>
	/// Only sends if the WebSocket is in a safe state.
	/// </summary>
	private async Task SafeSendAsync(string message, CancellationToken cancellation)
	{
		if (_socket.State != WebSocketState.Open)
			return;

		var bytes = Encoding.UTF8.GetBytes(message);
		await _socket.SendAsync(
			new ArraySegment<byte>(bytes),
			WebSocketMessageType.Text,
			true,
			cancellation
		);
	}

	public Task AckAsync<T>(T message, CancellationToken cancellation)
		=> Task.CompletedTask;

	public Task NackAsync<T>(T message, CancellationToken cancellation)
		=> Task.CompletedTask;

}
