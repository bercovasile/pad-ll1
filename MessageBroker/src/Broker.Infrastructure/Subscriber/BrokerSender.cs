using Broker.Application.Abstractions.Subscriber;
using Broker.Context.Response;
using Broker.Domain.Entites.Messages;
using Broker.Presentation.Core.Abstractions.Subscriber;
using System.Collections.Concurrent;

namespace Broker.Infrastructure.Subscriber;

public class BrokerSender(ISubscriptionManager subscriptionManager) : IBrokerSender
{

	private readonly ConcurrentDictionary<string, int> _topicIndex = new();


	public async Task<Response> PublishAsync(string topic, Message message, CancellationToken cancellation)
	{
		var subscribers = subscriptionManager.GetSubscribers(topic).ToList();
		if (!subscribers.Any())
		{
			return new Response
			{
				Success = false,
				Message = "No subscribers available."
			};
		}

		// Round-robin index
		var idx = _topicIndex.AddOrUpdate(topic, 0, (_, old) => (old + 1) % subscribers.Count);

		var subscriber = subscribers[idx];

		try
		{
			var response = await subscriber.SendAsync(message, cancellation);

			if (response.Success)
				return response;

			return new Response<Message>()
			{
				Data = message,
				Success = false
			};
		}
		catch (Exception ex)
		{
			return new Response<Message>()
			{
				Data = message,
				Success = false
			};
		}
	}
}
