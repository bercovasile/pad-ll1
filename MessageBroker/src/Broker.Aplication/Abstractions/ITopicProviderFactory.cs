

using Broker.Domain.Enums;

namespace Broker.Application.Abstractions;

public interface ITopicProviderFactory
{
	ITopicProvider Create(TopicBehavior behavior);
}
