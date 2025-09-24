
using Broker.Domain.Entites.Core;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Channels;

namespace Broker.Domain.Entites.Topics;

public abstract class Topic : BaseEntity
{
	public required string Name { get; set; }
	public TopicBehavior Behavior { get; protected set; }
	public DateTime LastMessageTime { get; set; } = DateTime.UtcNow;
	public long TotalMessagesProduced { get; set; } = 0;
	public long TotalMessagesConsumed { get; set; } = 0;
}


public class LogTopic : Topic
{
	public LogTopic() => Behavior = TopicBehavior.LogBased;

	public virtual List<Message> Messages { get; set; } = new();

	[Column(TypeName = "jsonb")]
	public Dictionary<string, long> ConsumerOffsets { get; set; } = new();

	public int Partitions { get; set; } = 1;
}

public class QueueTopic : Topic
{
	public QueueTopic() => Behavior = TopicBehavior.QueueBased;

	public virtual List<Message> PendingMessages { get; set; } = new();

	public Dictionary<string, bool> ConsumerAcks { get; set; } = new();

	public string ExchangeType { get; set; } = "direct";
	public bool Durable { get; set; } = true;
	public int DefaultPriority { get; set; } = 0;
}
