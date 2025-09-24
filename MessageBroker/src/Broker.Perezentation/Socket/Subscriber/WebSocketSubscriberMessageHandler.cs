using Broker.Application.Abstractions.Subscriber;

namespace Broker.Presentation.Socket.Subscriber;

public class WebSocketSubscriberMessageHandler
{
	private readonly IWebSocketSubscriberBroker _broker;

	public WebSocketSubscriberMessageHandler(IWebSocketSubscriberBroker broker)
	{
		_broker = broker;
	}

	public async Task HandleAsync(HttpContext context, CancellationToken cancellation = default)
	{
		var subscriber = await _broker.AcceptSubscriberAsync(context, cancellation);
		if (subscriber == null)
		{
			context.Response.StatusCode = 400;
			return;
		}

		// Subscriber-ul este acum activ, broker-ul poate trimite mesaje către el
		// Ack/Nack se va gestiona direct în CustomBroker
	}
}