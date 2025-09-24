

namespace Broker.Context.Messages;

public abstract class BaseModel
{
	public Dictionary<string, string>? Context { get; set; } = new();

}
