using Broker.Application.Abstractions;
using Broker.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Broker.Infrastructure.Services;

public class TopicProviderFactory : ITopicProviderFactory
{
	private readonly IReadOnlyDictionary<TopicBehavior, ITopicProvider> _providers;

	public TopicProviderFactory(IEnumerable<ITopicProvider> providers)
	{
		var duplicates = providers
			.GroupBy(p => p.SupportBehavior)
			.Where(g => g.Count() > 1)
			.Select(g => g.Key)
			.ToList();

		if (duplicates.Any())
			throw new InvalidOperationException($"Duplicate topic providers found for behaviors: {string.Join(", ", duplicates)}");

		_providers = providers.ToDictionary(p => p.SupportBehavior, p => p);
	}

	public ITopicProvider Create(TopicBehavior behavior)
	{
		if (_providers.TryGetValue(behavior, out var provider))
			return provider;

		throw new NotSupportedException($"No topic provider found for behavior {behavior}");
	}
}
