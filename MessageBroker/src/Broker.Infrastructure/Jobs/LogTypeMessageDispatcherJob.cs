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
		//private readonly IMongoBrokerContextFactory _mongoBrokerContextFactory;
		private readonly IDbContextFactory<BrokerPostgresContext> _postgrssBrokerContextFactory;

		private readonly BrokerConnection _brokerConnection;

        public LogTypeMessageDispatcherJob(IDbContextFactory<BrokerPostgresContext> postgrssBrokerContextFactory, BrokerConnection brokerConnection)
        {
			_postgrssBrokerContextFactory = postgrssBrokerContextFactory;
            _brokerConnection = brokerConnection;
        }

		// public async Task Execute(IJobExecutionContext context)
		// {
		//await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
		//{
		//    // Selectează 100 de mesaje log-based netrimise (Offset == null)
		//    var filter = Builders<Message>.Filter.Eq(m => m.Offset, null);
		//    var messages = await ctx.Messages.Find(filter).Limit(100).ToListAsync();
		//    foreach (var message in messages)
		//    {
		//        var topicConsumers = _brokerConnection.GetConsumers(message.TopicId.ToString());
		//        bool delivered = false;
		//        foreach (var consumer in topicConsumers)
		//        {
		//            var resp = await consumer.ConsumeAsync(message, CancellationToken.None);
		//            if (resp.Success)
		//            {
		//                delivered = true;
		//            }
		//        }
		//        if (delivered)
		//        {
		//            // Marchează ca trimis (setăm Offset)
		//            var update = Builders<Message>.Update.Set(m => m.Offset, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		//            await ctx.Messages.UpdateOneAsync(Builders<Message>.Filter.Eq(m => m.Id, message.Id), update);
		//        }
		//    }
		//});

		// }
		public async Task Execute(IJobExecutionContext context)
		{
			// Creează un nou context pentru job
			await using var dbContext = await _postgrssBrokerContextFactory.CreateDbContextAsync();

			// Selectează primele 100 de mesaje netrimise (Offset == null)
			var messages = await dbContext.Messages
				.Where(m => m.Offset == null)
				.OrderBy(m => m.CreatedAt) // presupun că există un câmp CreatedAt
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
					// Marchează mesajul ca trimis
					message.Offset = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
					dbContext.Messages.Update(message);
				}
			}

			// Salvează modificările în Postgres
			await dbContext.SaveChangesAsync();
		}
	}
}

