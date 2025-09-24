
using Broker.Application.Abstractions;
using Broker.Context.Messages;
using Broker.Context.Topics.Requests;
using Broker.Domain.Abstractions;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using Broker.Domain.Enums;
using Broker.Persistence.Contexts.Postgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Broker.Infrastructure.Services;

public class TopicProviderQueueBased : IQueueTopicProvider
{
	private readonly IMongoBrokerContextFactory _mongoBrokerContextFactory;
	private readonly ILogger<TopicProviderQueueBased> _logger;
	private readonly IMemoryCache _cache;
	public TopicBehavior SupportBehavior { get; private set; } = TopicBehavior.QueueBased;

	private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);
	public TopicProviderQueueBased(
		IMongoBrokerContextFactory mongoBrokerContextFactory,
		IMemoryCache cache,
		ILogger<TopicProviderQueueBased> logger)
	{
		_mongoBrokerContextFactory = mongoBrokerContextFactory;
		_cache = cache;
		_logger = logger;
	}

	public async Task<Guid> CreateTopicAsync(TopicRequest topic, CancellationToken cancellation = default)
	{
		
		if (await TopicExistsAsync(topic.Name, cancellation))
		{
			_logger.LogWarning("Topic {TopicName} already exists", topic.Name);
			throw new InvalidOperationException($"Topic {topic.Name} already exists.");
		}

		var queueTopic = new QueueTopic
		{
			Id = Guid.NewGuid(),
			Name = topic.Name,
			DefaultPriority = topic.DefaultPriority ?? 0,
			Durable = topic.Durable ?? true,
			ExchangeType = topic.ExchangeType ?? "direct",
			CreatedAt = DateTime.UtcNow
		};

		await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			await ctx.QueueTopics.InsertOneAsync(queueTopic, cancellationToken: cancellation);
		});

		_cache.Set(GetCacheKeyForTopic(topic.Name), queueTopic, CacheDuration);
		_cache.Set(GetCacheKeyForBehavior(topic.Name), topic.Behavior, CacheDuration);
		_cache.Set(GetCacheKeyForExistence(topic.Name), true, CacheDuration);

		return queueTopic.Id;
	}

	public async Task<Guid> DeleteTopicAsync(string topicName, CancellationToken cancellation = default)
	{
		var topic = await GetTopicAsync(topicName, cancellation);

		if (topic == null)
		{
			_logger.LogWarning("Topic {TopicName} not found", topicName);
			throw new KeyNotFoundException($"Topic {topicName} not found.");
		}

		// ștergere din MongoDB
		await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			await ctx.QueueTopics.DeleteOneAsync(t => t.Name == topicName, cancellation);
		});

		// curățare cache
		_cache.Remove(GetCacheKeyForTopic(topicName));
		_cache.Remove(GetCacheKeyForBehavior(topicName));
		_cache.Remove(GetCacheKeyForExistence(topicName));

		return topic.Id;
	}

	private async Task<Topic> GetTopicAsync(string topicName, CancellationToken cancellation = default)
	{
		if (_cache.TryGetValue<Topic>(GetCacheKeyForTopic(topicName), out var cachedTopic))
			return cachedTopic;

		return await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			var queueTopic = await ctx.QueueTopics
				.Find(t => t.Name == topicName)
				.FirstOrDefaultAsync(cancellation);

			if (queueTopic != null)
			{
				_cache.Set(GetCacheKeyForTopic(topicName), queueTopic, CacheDuration);
				_cache.Set(GetCacheKeyForBehavior(topicName), queueTopic.Behavior, CacheDuration);
				_cache.Set(GetCacheKeyForExistence(topicName), true, CacheDuration);
			}

			return queueTopic;
		}) ?? throw new KeyNotFoundException($"Topic {topicName} not found.");
	}

	private async Task<bool> TopicExistsAsync(string topicName, CancellationToken cancellation = default)
	{
		if (_cache.TryGetValue<bool?>(GetCacheKeyForExistence(topicName), out var cachedExists))
			return cachedExists!.Value;

		var existsInMongo = await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			return await ctx.QueueTopics.Find(t => t.Name == topicName).AnyAsync(cancellation);
		});

		_cache.Set(GetCacheKeyForExistence(topicName), existsInMongo, CacheDuration);
		return existsInMongo;
	}

	private static string GetCacheKeyForTopic(string topicName) => $"topic:{topicName}";
	private static string GetCacheKeyForBehavior(string topicName) => $"topic:behavior:{topicName}";
	private static string GetCacheKeyForExistence(string topicName) => $"topic:exists:{topicName}";

	public async Task<Guid> AddMessageAsync(string topicName, MessageRequest message, CancellationToken cancellation = default)
	{
		
		var topic = await GetTopicAsync(topicName, cancellation);

		if (topic == null)
			throw new KeyNotFoundException($"Topic {topicName} not found.");

		var messageEntity = new Message
		{
			Id = Guid.NewGuid(),
			Key = message.Key,
			Value = message.Value,
			Timestamp = DateTime.UtcNow,
			TopicId = topic.Id,	
			Headers = message.Headers ?? new Dictionary<string, string>(),
			Priority = message.Priority 
		};

		await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		{
			await ctx.Messages.InsertOneAsync(messageEntity, cancellationToken: cancellation);

			// opțional: update pe topic (ex. nr. total mesaje, ultima activitate)
			var update = Builders<QueueTopic>.Update
				.Inc(t => t.TotalMessagesProduced, 1)
				.Set(t => t.LastMessageTime, DateTime.UtcNow);

			await ctx.QueueTopics.UpdateOneAsync(t => t.Id == topic.Id, update, cancellationToken: cancellation);
		});

		return messageEntity.Id;
	}

}
