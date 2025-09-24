

using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using MongoDB.Driver;

namespace Broker.Domain.Abstractions;

public interface IMongoBrokerContext : IDisposable
{
	IMongoCollection<QueueTopic> QueueTopics { get; }
	IMongoCollection<Message> Messages { get; }
	bool IsActive { get; }
	string ConnectionId { get; }
}