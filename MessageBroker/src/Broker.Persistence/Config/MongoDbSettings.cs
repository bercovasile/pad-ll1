namespace Broker.Persistence.Config;

public class MongoDbSettings
{
	public string ConnectionString { get; set; } = string.Empty;
	public string DatabaseName { get; set; } = string.Empty;
	public string QueueTopicsCollectionName { get; set; } = "QueueTopics";
	public string MessagesCollectionName { get; set; } = "Messages";
	public int MaxPoolSize { get; set; } = 20;
	public int MinPoolSize { get; set; } = 5;

	public void Validate()
	{
		if (string.IsNullOrWhiteSpace(ConnectionString))
			throw new InvalidOperationException("ConnectionString is required");

		if (string.IsNullOrWhiteSpace(DatabaseName))
			throw new InvalidOperationException("DatabaseName is required");

		if (MaxPoolSize < MinPoolSize)
			throw new InvalidOperationException("MaxPoolSize must be greater than or equal to MinPoolSize");
	}
}