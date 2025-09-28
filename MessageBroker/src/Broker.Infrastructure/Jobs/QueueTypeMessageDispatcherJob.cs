using System;
using System.Threading.Tasks;
using Quartz;
using Broker.Infrastructure.Services;
using Broker.Domain.Entites.Messages;
using MongoDB.Driver;
using Broker.Domain.Abstractions;
using System.Linq;
using System.Threading;

namespace Broker.Infrastructure.Jobs
{
    public class QueueTypeMessageDispatcherJob : IJob
    {
        private readonly IMongoBrokerContextFactory _mongoBrokerContextFactory;
        private readonly BrokerConnection _brokerConnection;

        public QueueTypeMessageDispatcherJob(IMongoBrokerContextFactory mongoBrokerContextFactory, BrokerConnection brokerConnection)
        {
            _mongoBrokerContextFactory = mongoBrokerContextFactory;
            _brokerConnection = brokerConnection;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _mongoBrokerContextFactory.ExecuteAsync(async ctx =>
            {
                // Selectează 100 de mesaje queue-based neacknowledged
                var filter = Builders<Message>.Filter.Eq(m => m.Acknowledged, false);
                var messages = await ctx.Messages.Find(filter).Limit(100).ToListAsync();
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
                        var update = Builders<Message>.Update.Set(m => m.Acknowledged, true);
                        await ctx.Messages.UpdateOneAsync(Builders<Message>.Filter.Eq(m => m.Id, message.Id), update);
                    }
                }
            });
        }
    }
}
