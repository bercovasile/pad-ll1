using Broker.Domain.Abstractions;
using Broker.Domain.Entites.Messages;
using Broker.Infrastructure.Services;
using Broker.Persistence.Contexts.Postgress;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Quartz;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Infrastructure.Jobs
{
    public class LogTypeMessageDispatcherJob : IJob
    {
		private readonly IDbContextFactory<BrokerPostgresContext> _postgrssBrokerContextFactory;

		private readonly BrokerConnection _brokerConnection;


		public LogTypeMessageDispatcherJob(IDbContextFactory<BrokerPostgresContext> postgrssBrokerContextFactory, BrokerConnection brokerConnection)
        {
			_postgrssBrokerContextFactory = postgrssBrokerContextFactory;
            _brokerConnection = brokerConnection;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			await using var dbContext = await _postgrssBrokerContextFactory.CreateDbContextAsync();

			
			var messages = await dbContext.Messages
				.Where(m => m.Offset == null)
				.Include(m => m.LogTopic)
				.OrderBy(m => m.CreatedAt) 
				.Take(100)
				.ToListAsync();

			foreach (var message in messages)
			{
				var topicConsumers = _brokerConnection.GetConsumers(message.TopicId.ToString());
				bool delivered = false;

				foreach (var consumer in topicConsumers)
				{
					var resp = await consumer.ConsumeAsync(message, CancellationToken.None);
					if (resp.Success)
					{
						delivered = true;
					}

				}
				if (delivered)
				{
					message.Offset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					dbContext.Messages.Update(message);
				}
			}
			await dbContext.SaveChangesAsync();
		}
	}
}

