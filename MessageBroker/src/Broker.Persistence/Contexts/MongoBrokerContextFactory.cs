using Broker.Domain.Abstractions;
using Broker.Persistence.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace Broker.Persistence.Contexts;

// ===== POOLED CONTEXT FACTORY =====
public class MongoBrokerContextFactory : IMongoBrokerContextFactory, IDisposable
{
	private readonly ObjectPool<MongoBrokerContext> _contextPool;
	private readonly ILogger<MongoBrokerContextFactory>? _logger;
	private readonly MongoDbSettings _settings;
	private volatile bool _disposed = false;

	public MongoBrokerContextFactory(MongoDbSettings settings, ILogger<MongoBrokerContextFactory>? logger = null)
	{
		_settings = settings ?? throw new ArgumentNullException(nameof(settings));
		_logger = logger;

		var policy = new MongoBrokerContextPooledObjectPolicy(settings, logger);
		var provider = new DefaultObjectPoolProvider();
		_contextPool = provider.Create(policy);

		_logger?.LogInformation("MongoBrokerContextFactory initialized with connection: {ConnectionString}",
			settings.ConnectionString);
	}

	public IMongoBrokerContext GetContext()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(MongoBrokerContextFactory));

		var context = _contextPool.Get();
		_logger?.LogDebug("Context {ConnectionId} retrieved from pool", context.ConnectionId);
		return context;
	}

	public void ReturnContext(IMongoBrokerContext context)
	{
		if (_disposed || context == null)
			return;

		if (context is MongoBrokerContext pooledContext)
		{
			_contextPool.Return(pooledContext);
		}
	}

	public async Task<T> ExecuteAsync<T>(Func<IMongoBrokerContext, Task<T>> operation)
	{
		var context = GetContext();
		try
		{
			return await operation(context).ConfigureAwait(false);
		}
		finally
		{
			ReturnContext(context);
		}
	}

	public async Task ExecuteAsync(Func<IMongoBrokerContext, Task> operation)
	{
		var context = GetContext();
		try
		{
			await operation(context).ConfigureAwait(false);
		}
		finally
		{
			ReturnContext(context);
		}
	}

	public void Dispose()
	{
		if (_disposed)
			return;

		_disposed = true;

		if (_contextPool is IDisposable disposablePool)
		{
			disposablePool.Dispose();
		}

		_logger?.LogInformation("MongoBrokerContextFactory disposed");
	}
}