using Broker.Context.Response;
using Broker.Domain.Entites.Messages;

namespace Broker.Application.Abstractions.Subscriber;

public interface IMessageSubscriber
{
	ITopicContext Context { get; }
	Task<Response> SendAsync(Message message  , CancellationToken cancellation) ;
}