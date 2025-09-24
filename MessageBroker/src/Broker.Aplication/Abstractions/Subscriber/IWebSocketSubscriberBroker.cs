using Microsoft.AspNetCore.Http;

namespace Broker.Application.Abstractions.Subscriber;

public interface IWebSocketSubscriberBroker
{
	Task<IMessageSubscriber?> AcceptSubscriberAsync(HttpContext context, CancellationToken cancellation = default);
}
