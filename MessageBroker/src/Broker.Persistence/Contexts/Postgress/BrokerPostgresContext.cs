using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Topics;
using Broker.Persistence.Contexts.Postgres;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Broker.Persistence.Contexts.Postgress;


public class BrokerPostgresContext(DbContextOptions<BrokerPostgresContext> options ) :
	DbContextBase(options, defaultSchema: "public")
{
	
	public DbSet<LogTopic> LogTopics => Set<LogTopic>();
	public DbSet<Message> Messages => Set<Message>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Message>()
			.HasIndex(m => m.Timestamp);

		modelBuilder.Entity<Message>()
			.HasIndex(m => m.Offset);


		modelBuilder.Entity<Message>()
			.HasOne(m => m.LogTopic)
			.WithMany(t => t.Messages)
			.HasForeignKey(m => m.TopicId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
