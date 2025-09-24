using Broker.Application.Abstractions.Subscriber;
using Broker.Infrastructure.Core;
using Broker.Infrastructure.Subscriber;
using Broker.Presentation.Core.Abstractions.Subscriber;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net.WebSockets;

namespace Broker.Presentation.Socket.Subscriber;


public class WebSocketSubscriberBroker : IWebSocketSubscriberBroker
{
	private readonly ISubscriptionManager _subscriptionManager;

	public WebSocketSubscriberBroker( ISubscriptionManager subscriptionManager)
	{
		_subscriptionManager = subscriptionManager;
	}

	public async Task<IMessageSubscriber?> AcceptSubscriberAsync(HttpContext context, CancellationToken cancellation = default)
	{
		if (!context.WebSockets.IsWebSocketRequest)
			return null;

		var topic = context.GetRouteValue("topic")?.ToString()
		   ?? context.Request.Query["topic"].ToString()
		   ?? Guid.NewGuid().ToString();

		var socket = await context.WebSockets.AcceptWebSocketAsync();

		var subscriber = new WebSocketSubscriber(socket, new TopicContext(topic));

		_subscriptionManager.RegisterSubscriber(subscriber);

		_ = Task.Run(async () =>
		{
			var buffer = new byte[1024 * 4];
			try
			{
				while (socket.State == WebSocketState.Open)
				{
					var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellation);
					if (result.MessageType == WebSocketMessageType.Close)
					{
						await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", cancellation);
						break;
					}
				}
			}
			finally
			{
				_subscriptionManager.UnregisterSubscriber(subscriber);
			}
		}, cancellation);

		return subscriber;
	}
}
