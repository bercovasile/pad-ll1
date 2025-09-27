
using Broker.Domain.Entites.Core;
using Broker.Domain.Entites.Topics;
using System.ComponentModel.DataAnnotations.Schema;

namespace Broker.Domain.Entites.Messages;
public class Message : BaseEntity
{
	public required string Key { get; set; }
	public required string Value { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;

	[Column(TypeName = "jsonb")]
	public Dictionary<string, string> Headers { get; set; }

	// Log-based
	public long? Offset { get; set; }
	public string? PartitionKey { get; set; }

	// Queue-based
	public bool Acknowledged { get; set; } = false;


	public int Priority { get; set; } = 0;

	public Guid TopicId { get; set; }
	[ForeignKey(nameof(TopicId))]
	public virtual LogTopic LogTopic { get; set; } = null!;


}

