using Broker.Context.Messages;
using Broker.Context.Topics.Enums;

namespace Broker.Context.Topics.Requests;

public class TopicRequest : BaseModel
{
	public string Name { get; set; } = string.Empty;
	public TopicBehavior Behavior { get;  set; } = TopicBehavior.None;

	//LogTopic
	public int Partitions { get; set; } = 1;

	//QueueTopic
	public string? ExchangeType { get; set; } = "direct";
	public bool? Durable { get; set; } = true;
	public int? DefaultPriority { get; set; } = 0;

}
