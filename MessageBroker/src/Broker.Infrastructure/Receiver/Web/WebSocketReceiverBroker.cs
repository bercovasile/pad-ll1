using Broker.Application.Abstractions.Receiver;
using Broker.Infrastructure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Broker.Infrastructure.Receiver.Web;

public class WebSocketReceiverBroker : IWebSocketReceiverBroker
{
	private readonly IServiceProvider _provider;

	public WebSocketReceiverBroker(IServiceProvider provider)
	{
		_provider = provider;
	}

	public async Task<IBrokerReceiver?> AcceptReceiverAsync(HttpContext context, CancellationToken cancellation = default)
	{
		if (!context.WebSockets.IsWebSocketRequest)
			return null;

		// Example: /web-socket/{topic} or /web-socket?topic=myTopic
		var topic = context.GetRouteValue("topic")?.ToString()
		   ?? context.Request.Query["topic"].ToString()
		   ?? Guid.NewGuid().ToString();



		var socket = await context.WebSockets.AcceptWebSocketAsync();

		var receiver = _provider.GetRequiredService<IBrokerReceiver>();

		if (receiver is WebSocketMessageReceiver socketReceiver)
		{
			socketReceiver.SetWebSocket(socket);
			socketReceiver.SetTopic(new TopicContext(topic)); 
		
		}

		return receiver;
	}
}
