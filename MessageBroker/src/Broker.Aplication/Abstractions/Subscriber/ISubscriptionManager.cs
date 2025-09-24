using Broker.Application.Abstractions.Subscriber;

namespace Broker.Presentation.Core.Abstractions.Subscriber;

public interface ISubscriptionManager
{
	void RegisterSubscriber(IMessageSubscriber subscriber);
	void UnregisterSubscriber(IMessageSubscriber subscriber);
	IEnumerable<IMessageSubscriber> GetSubscribers(string topic);
}
