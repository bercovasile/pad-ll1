

using Broker.Persistence.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Broker.Persistence.Contexts;

public class MongoBrokerContextPooledObjectPolicy : IPooledObjectPolicy<MongoBrokerContext>
{
	private readonly MongoDbSettings _settings;
	private readonly ILogger? _logger;
	private static long _contextCounter = 0;

	public MongoBrokerContextPooledObjectPolicy(MongoDbSettings settings, ILogger? logger = null)
	{
		_settings = settings;
		_logger = logger;
	}

	public MongoBrokerContext Create()
	{
		var connectionId = $"mongo-ctx-{Interlocked.Increment(ref _contextCounter)}";
		_logger?.LogDebug("Creating new MongoDB context: {ConnectionId}", connectionId);

		return new MongoBrokerContext(_settings, connectionId, _logger);
	}

	public bool Return(MongoBrokerContext obj)
	{
		if (obj == null || !obj.IsActive)
		{
			_logger?.LogWarning("Rejecting inactive context {ConnectionId} from pool", obj?.ConnectionId ?? "unknown");
			return false;
		}

		_logger?.LogDebug("Context {ConnectionId} accepted back into pool", obj.ConnectionId);
		return true;
	}
}