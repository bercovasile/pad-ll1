
using Broker.Application.Abstractions;
using Broker.Context.Messages;
using Broker.Context.Topics.Requests;
using Broker.Domain.Abstractions;
using Broker.Domain.Entites.Core;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using Broker.Domain.Enums;
using Broker.Persistence.Contexts.Postgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;

namespace Broker.Infrastructure.Services;

public class TopicProviderLogBased : ILogTopicProvider
{
	private readonly IDbContextFactory<BrokerPostgresContext> _dbContextFactory;
	private readonly ILogger<TopicProviderLogBased> _logger;
	private readonly IMemoryCache _cache;

	private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

	public TopicBehavior SupportBehavior => TopicBehavior.LogBased;

	public TopicProviderLogBased(
		IDbContextFactory<BrokerPostgresContext> dbContextFactory,
		ILogger<TopicProviderLogBased> logger,
		IMemoryCache cache)
	{
		_dbContextFactory = dbContextFactory;
		_logger = logger;
		_cache = cache;
	}


	public async Task<Guid> CreateTopicAsync(TopicRequest topic, CancellationToken cancellation = default)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);

		if (await db.LogTopics.AnyAsync(t => t.Name == topic.Name, cancellation))
			throw new InvalidOperationException($"Topic {topic.Name} already exists.");

		var logTopic = new LogTopic
		{
			Id = Guid.NewGuid(),
			Name = topic.Name,
			Partitions = topic.Partitions,
		};

		await db.LogTopics.AddAsync(logTopic, cancellation);
		await db.SaveChangesAsync(cancellation);

		_cache.Set(GetCacheKeyForTopic(topic.Name), logTopic, CacheDuration);
		_cache.Set(GetCacheKeyForBehavior(topic.Name), topic.Behavior, CacheDuration);
		_cache.Set(GetCacheKeyForExistence(topic.Name), true, CacheDuration);

		return logTopic.Id;
	}

	public async Task<Guid> DeleteTopicAsync(string topicName, CancellationToken cancellation = default)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);

		var topic = await db.LogTopics.FirstOrDefaultAsync(t => t.Name == topicName, cancellation);

		if (topic == null)
			throw new KeyNotFoundException($"Topic {topicName} not found.");

		db.LogTopics.Remove(topic);
		await db.SaveChangesAsync(cancellation);

		_cache.Remove(GetCacheKeyForTopic(topicName));
		_cache.Remove(GetCacheKeyForBehavior(topicName));
		_cache.Remove(GetCacheKeyForExistence(topicName));

		return topic.Id;
	}

	private static string GetCacheKeyForTopic(string topicName) => $"topic:{topicName}";
	private static string GetCacheKeyForBehavior(string topicName) => $"topic:behavior:{topicName}";
	private static string GetCacheKeyForExistence(string topicName) => $"topic:exists:{topicName}";

	public async Task<Guid> AddMessageAsync(string topicName, MessageRequest message, CancellationToken cancellation = default)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellation);


		var topic = await db.LogTopics.FirstOrDefaultAsync(t => t.Name == topicName, cancellation);
	
		if (topic == null)
			throw new KeyNotFoundException($"Topic {topicName} not found.");
		topic.TotalMessagesProduced += 1;
		topic.LastMessageTime = DateTime.UtcNow;
	

		var messageEntity = new Message
		{
			Id = Guid.NewGuid(),
			Key = message.Key,
			Value = message.Value,
			Timestamp = DateTime.UtcNow,
			TopicId = topic.Id,
			Headers = message.Headers ?? new Dictionary<string, string>(),
		};

		await db.Messages.AddAsync(messageEntity, cancellation);

		await db.SaveChangesAsync(cancellation);
		return messageEntity.Id;
	}
}
