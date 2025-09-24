using Broker.Context.Response;
using Broker.Domain.Entites.Messages;

namespace Broker.Application.Abstractions.Subscriber;

public interface IBrokerSender
{
	Task<Response> PublishAsync(string topic, Message message, CancellationToken cancellation);

}
