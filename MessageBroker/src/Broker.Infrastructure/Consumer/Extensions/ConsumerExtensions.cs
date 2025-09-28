using System;
using System.Reactive.Linq;
using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Messages;

namespace Broker.Infrastructure.Consumer.Extensions
{
	public static class ConsumerExtensions
	{
		public static IObservable<Message> Where(this IMessageConsumer consumer, Func<Message, bool> predicate)
		{
			// Map MessageAcknowledgment to Message, then filter
			return consumer.Acks
				.Select(a => new Message
				{
					Id = a.MessageId,
					Key = string.Empty,
					Value = string.Empty,
					Timestamp = DateTime.MinValue,
					Headers = new Dictionary<string, string>(),
					Priority = 0,
					TopicId = Guid.Empty,
					LogTopic = null
				})
				.Where(predicate);
		}
	}
}
