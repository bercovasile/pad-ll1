using Microsoft.AspNetCore.Http;

namespace Broker.Application.Abstractions.Receiver;

public interface IWebSocketReceiverBroker
{
	Task<IBrokerReceiver> AcceptReceiverAsync(HttpContext context, CancellationToken cancellation = default);
}

