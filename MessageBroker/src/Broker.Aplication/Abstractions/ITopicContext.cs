namespace Broker.Application.Abstractions;

public interface ITopicContext
{
	/// <summary>
	/// The topic associated with this client connection.
	/// </summary>
	string ContextTopic { get; }

	/// <summary>
	/// Returns the topic name for the current client connection.
	/// </summary>
	string GetTopicName();
}