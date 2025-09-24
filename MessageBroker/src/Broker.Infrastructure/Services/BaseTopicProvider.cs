using Broker.Application.Abstractions;
using Broker.Domain.Abstractions;
using Broker.Domain.Entites.Topics;
using Broker.Domain.Enums;
using Broker.Persistence.Contexts.Postgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Broker.Infrastructure.Services;

public class BaseTopicProvider : IBaseTopicProvider
{
	private readonly IDbContextFactory<BrokerPostgresContext> _dbContextFactory;
	private readonly IMongoBrokerContextFactory _mongoBrokerContextFactory;
	private readonly ILogger<BaseTopicProvider> _logger;
	private readonly IMemoryCache _cache;

	private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
	public BaseTopicProvider(
		IDbContextFactory<BrokerPostgresContext> dbContextFactory,
		IMongoBrokerContextFactory mongoBrokerContextFactory,
		IMemoryCache cache,
		ILogger<BaseTopicProvider> logger)
	{
		_dbContextFactory = dbContextFactory;
		_mongoBrokerContextFactory = mongoBrokerContextFactory;
		_cache = cache;
		_logger = logger;
	}
	public async Task<Topic> GetTopicAsync(string topicName, CancellationToken cancellation = default)
	{
		if (_cache.TryGetValue<Topic>(GetCacheKeyForTopic(topicName), out var cachedTopic))
			return cachedTopic;

		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);

		var logTopic = await db.LogTopics
			.Include(t => t.Messages)
			.FirstOrDefaultAsync(t => t.Name == topicName, cancellation);

		if (logTopic != null)
		{
			_cache.Set(GetCacheKeyForTopic(topicName), logTopic, CacheDuration);
			return logTopic;
		}

		return await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			var queueTopic = await ctx.QueueTopics
				.Find(t => t.Name == topicName)
				.FirstOrDefaultAsync(cancellation);

			if (queueTopic != null)
			{
				_cache.Set(GetCacheKeyForTopic(topicName), (Topic)queueTopic, CacheDuration);
				return (Topic)queueTopic;
			}

			_logger.LogWarning("Topic {TopicName} not found", topicName);
			throw new KeyNotFoundException($"Topic {topicName} not found.");
		});
	}

	public async Task<TopicBehavior?> GetTopicBehaviorAsync(string topicName, CancellationToken cancellation = default)
	{
		if (_cache.TryGetValue<TopicBehavior?>(GetCacheKeyForBehavior(topicName), out var cachedBehavior))
			return cachedBehavior;

		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);

		var logTopic = await db.LogTopics
			.Where(t => t.Name == topicName)
			.Select(t => t.Behavior)
			.FirstOrDefaultAsync(cancellation);

		if (logTopic != default)
		{
			_cache.Set(GetCacheKeyForBehavior(topicName), logTopic, CacheDuration);
			return logTopic;
		}

		return await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			var queueTopic = await ctx.QueueTopics
				.Find(t => t.Name == topicName)
				.FirstOrDefaultAsync(cancellation);

			if (queueTopic != null)
			{
				var behavior = ((Topic)queueTopic).Behavior;
				_cache.Set(GetCacheKeyForBehavior(topicName), behavior, CacheDuration);
				return behavior;
			}

			_logger.LogWarning("Topic {TopicName} not found", topicName);
			throw new KeyNotFoundException($"Topic {topicName} not found.");
		});
	}

	public async Task<bool> TopicExistsAsync(string topicName, CancellationToken cancellation = default)
	{
		if (_cache.TryGetValue<bool?>(GetCacheKeyForExistence(topicName), out var cachedExists))
			return cachedExists!.Value;

		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);

		var existsInPostgres = await db.LogTopics
			.AnyAsync(t => t.Name == topicName, cancellation);

		if (existsInPostgres)
		{
			_cache.Set(GetCacheKeyForExistence(topicName), true, CacheDuration);
			return true;
		}

		return await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			var existsInMongo = await ctx.QueueTopics
				.Find(t => t.Name == topicName)
				.AnyAsync(cancellation);

			_cache.Set(GetCacheKeyForExistence(topicName), existsInMongo, CacheDuration);
			return existsInMongo;
		});
	}

	private static string GetCacheKeyForTopic(string topicName) => $"topic:{topicName}";
	private static string GetCacheKeyForBehavior(string topicName) => $"topic:behavior:{topicName}";
	private static string GetCacheKeyForExistence(string topicName) => $"topic:exists:{topicName}";
}