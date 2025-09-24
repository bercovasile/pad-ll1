using Broker.Application.Abstractions.Subscriber;
using Broker.Presentation.Core.Abstractions.Subscriber;
using System.Collections.Concurrent;

namespace Broker.Presentation.Core.Subscriber;

public class SubscriptionManager : ISubscriptionManager
{
	private readonly ConcurrentDictionary<string, HashSet<IMessageSubscriber>> _topics = new();

	public void RegisterSubscriber(IMessageSubscriber subscriber)
	{
		var set = _topics.GetOrAdd(subscriber.Context.ContextTopic, _ => new HashSet<IMessageSubscriber>());
		lock (set)
		{
			set.Add(subscriber);
		}
	}

	public void UnregisterSubscriber(IMessageSubscriber subscriber)
	{
		if (_topics.TryGetValue(subscriber.Context.ContextTopic, out var set))
		{
			lock (set)
			{
				set.Remove(subscriber);
			}
		}
	}

	public IEnumerable<IMessageSubscriber> GetSubscribers(string topic)
	{
		if (_topics.TryGetValue(topic, out var set))
		{
			lock (set)
			{
				return set.ToList();
			}
		}
		return Enumerable.Empty<IMessageSubscriber>();
	}
}