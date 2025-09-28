using Broker.Domain.Abstractions;
using Broker.Domain.Entites.Core;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using Broker.Persistence.Config;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Broker.Persistence.Contexts;


public class MongoBrokerContext : IMongoBrokerContext
{
	private readonly MongoClient _client;
	private readonly IMongoDatabase _database;
	private readonly ILogger? _logger;
	private volatile bool _disposed = false;
	private volatile bool _isActive = true;

	public string ConnectionId { get; }
	public bool IsActive => _isActive && !_disposed;

	// Static constructor runs once per AppDomain, avoiding repeated serializer registration.
	static MongoBrokerContext()
	{
		try
		{
			BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
		}
		catch (BsonSerializationException)
		{
			// Serializer already registered — ignore.
		}
	}

	public MongoBrokerContext(MongoDbSettings settings, string connectionId, ILogger? logger = null)
	{
		ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
		_logger = logger;

		// Validare parametri
		if (settings == null)
			throw new ArgumentNullException(nameof(settings));

		if (string.IsNullOrWhiteSpace(settings.ConnectionString))
			throw new ArgumentException("Connection string cannot be null or empty", nameof(settings));

		if (string.IsNullOrWhiteSpace(settings.DatabaseName))
			throw new ArgumentException("Database name cannot be null or empty", nameof(settings));

		try
		{
			var clientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);				
			clientSettings.ConnectTimeout = TimeSpan.FromSeconds(10);
			clientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
			clientSettings.SocketTimeout = TimeSpan.FromSeconds(30);
			clientSettings.MaxConnectionPoolSize = 100;
			clientSettings.MinConnectionPoolSize = 5;
			clientSettings.MaxConnectionIdleTime = TimeSpan.FromMinutes(30);
			clientSettings.WaitQueueTimeout = TimeSpan.FromSeconds(5);
			

			_client = new MongoClient(clientSettings);

			_database = _client.GetDatabase(settings.DatabaseName);


			//if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity)))
			//{
			//	BsonClassMap.RegisterClassMap<BaseEntity>(cm =>
			//	{
			//		cm.AutoMap();
			//		cm.MapIdProperty(c => c.Id)
			//		  .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
			//	});
			//}

		
			// Inițializare colecții
			QueueTopics = _database.GetCollection<QueueTopic>(
				settings.QueueTopicsCollectionName ?? "QueueTopics");
			Messages = _database.GetCollection<Message>(
				settings.MessagesCollectionName ?? "Messages");

			_logger?.LogDebug("MongoDB context {ConnectionId} initialized successfully", ConnectionId);
		}
		catch (Exception ex)
		{
			_isActive = false;
			_logger?.LogError(ex, "Failed to initialize MongoDB context {ConnectionId}", ConnectionId);
			throw;
		}
	}

	public IMongoCollection<QueueTopic> QueueTopics { get; private set; } = null!;
	public IMongoCollection<Message> Messages { get; private set; } = null!;

	public async Task<bool> ValidateConnectionAsync()
	{
		if (!_isActive || _disposed)
			return false;

		try
		{
			await _database.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
			return true;
		}
		catch (Exception ex)
		{
			_logger?.LogWarning(ex, "Connection validation failed for context {ConnectionId}", ConnectionId);
			_isActive = false;
			return false;
		}
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;
		_isActive = false;

		try
		{
			_client?.Dispose();
			_logger?.LogDebug("MongoDB context {ConnectionId} disposed", ConnectionId);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Error disposing MongoDB context {ConnectionId}", ConnectionId);
		}
	}
}
