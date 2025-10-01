

using System.ComponentModel.DataAnnotations.Schema;

namespace Broker.Context.Messages;

public class MessageRequest  : BaseModel
{
	public string Key { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;
	public Dictionary<string, string> Headers { get; set; } = new();
	public int Priority { get; set; } = 0;

	public string? TopicName { get; set; } = string.Empty;
}
