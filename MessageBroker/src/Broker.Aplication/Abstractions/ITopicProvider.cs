using Broker.Context.Messages;
using Broker.Context.Topics.Requests;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using Broker.Domain.Enums;

namespace Broker.Application.Abstractions
{
	public interface IBaseTopicProvider
	{

		
		Task<TopicBehavior?> GetTopicBehaviorAsync(string topicName, CancellationToken cancellation = default);
		Task<bool> TopicExistsAsync(string topicName, CancellationToken cancellation = default);
		Task<Topic> GetTopicAsync(string topicName, CancellationToken cancellation = default);


	}


	public interface ITopicProvider 
	{
		TopicBehavior SupportBehavior { get; }
		Task<Guid> CreateTopicAsync( TopicRequest topic ,  CancellationToken cancellation = default);
		Task<Guid> DeleteTopicAsync(string topicName, CancellationToken cancellation = default);

		Task<Guid> AddMessageAsync(string topicName, MessageRequest message, CancellationToken cancellation = default);

	}

	public interface IQueueTopicProvider : ITopicProvider
	{
		new TopicBehavior SupportBehavior => TopicBehavior.QueueBased;
		

	}	

	public interface ILogTopicProvider : ITopicProvider
	{
		new TopicBehavior SupportBehavior => TopicBehavior.LogBased;
		
	}

}
