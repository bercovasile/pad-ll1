
using Broker.Application.Abstractions;
using Broker.Application.Abstractions.Subscriber;
using Broker.Context.Response;
using Broker.Domain.Entites.Messages;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Broker.Infrastructure.Subscriber;

public class WebSocketSubscriber : IMessageSubscriber
{
	private readonly WebSocket _socket;
	public ITopicContext Context { get; }

	public WebSocketSubscriber(WebSocket socket, ITopicContext context)
	{
		_socket = socket ?? throw new ArgumentNullException(nameof(socket));
		Context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<Response> SendAsync(Message message, CancellationToken cancellation)
	{
		if (_socket.State != WebSocketState.Open)
		{
			return Response<Message>.Fail(message);
			
		}

		try
		{
			var payload = JsonSerializer.Serialize(message);
			var bytes = Encoding.UTF8.GetBytes(payload);

			await _socket.SendAsync(
				new ArraySegment<byte>(bytes),
				WebSocketMessageType.Text,
				true,
				cancellation
			);

			// Ack imediat după trimitere (poți schimba să aștepți confirmarea de la client)
			return Response<Message>.Ok(message);
		}
		catch (Exception ex)
		{
			return Response<Message>.Fail(message);
		}
		
	}
}
