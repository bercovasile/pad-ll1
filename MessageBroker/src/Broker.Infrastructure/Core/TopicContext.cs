using Broker.Application.Abstractions;

namespace Broker.Infrastructure.Core;

public class TopicContext : ITopicContext
{
	private readonly string _topic;

	public TopicContext(string topic)
	{
		_topic = topic;
	}

	public string ContextTopic => _topic;

	public string GetTopicName() => _topic;
}