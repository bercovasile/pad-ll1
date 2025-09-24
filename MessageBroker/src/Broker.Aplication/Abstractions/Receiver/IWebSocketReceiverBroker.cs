using Microsoft.AspNetCore.Http;

namespace Broker.Application.Abstractions.Receiver;

public interface IWebSocketReceiverBroker
{
	Task<IMessageReceiver> AcceptReceiverAsync(HttpContext context, CancellationToken cancellation = default);
}

