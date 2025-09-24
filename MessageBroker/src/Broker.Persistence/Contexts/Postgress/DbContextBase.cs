using Broker.Domain.Entites.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

using System.Data;
using System.Data.Common;

namespace Broker.Persistence.Contexts.Postgres;

public abstract class DbContextBase : DbContext
{
	public readonly string _defaultSchema;

	protected DbContextBase(
		DbContextOptions options,
	
		string defaultSchema = "public")
		: base(options)
	{
		_defaultSchema = defaultSchema;
	}

	public override int SaveChanges()
	{
		ApplyBaseEntityRules();
		var result = base.SaveChanges();
		//ApplyIHasDomainEventsRules();
		return result;
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		ApplyBaseEntityRules();
		var result = await base.SaveChangesAsync(cancellationToken);
		//_ = ApplyIHasDomainEventsRulesAsync(cancellationToken);

		return result;
	}


	private void ApplyBaseEntityRules()
	{
		DateTime utcNow = DateTime.UtcNow;
		foreach (EntityEntry<BaseEntity> item in ChangeTracker.Entries<BaseEntity>())
		{
			if (item.State == EntityState.Added)
			{
				item.Entity.CreatedAt = utcNow;
				item.Entity.UpdatedAt = utcNow;
			}

			if (item.State == EntityState.Modified)
			{
				item.Entity.UpdatedAt = utcNow;
			}
		}
	}

	

	//private async Task ApplyIHasDomainEventsRulesAsync(CancellationToken cancellationToken = default)
	//{
	//	var utcNow = DateTime.UtcNow;

	//	var domainEntities = ChangeTracker
	//		.Entries<IHasDomainEvents>()
	//		.Where(x => x.Entity.HasEvents)
	//		.Select(x => x.Entity)
	//		.ToList();

	//	var domainEvents = domainEntities.SelectMany(e => e.Events).ToList();

	//	if (domainEvents.Any())
	//	{
	//		//await _domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);
	//		domainEntities.ForEach(e => e.ClearEvents());
	//	}
	//}
	//private void ApplyIHasDomainEventsRules()
	//{

	//	var domainEntities = ChangeTracker
	//		.Entries<IHasDomainEvents>()
	//		.Where(x => x.Entity.HasEvents)
	//		.Select(x => x.Entity)
	//		.ToList();

	//	var domainEvents = domainEntities.SelectMany(e => e.Events).ToList();

	//	if (domainEvents.Any())
	//	{
	//		Task.Run(() => _domainEventsDispatcher.DispatchAsync(domainEvents));
	//		domainEntities.ForEach(e => e.ClearEvents());
	//	}
	//}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema(_defaultSchema);
		modelBuilder.HasPostgresExtension("uuid-ossp");
		foreach (IMutableEntityType item in from e in modelBuilder.Model.GetEntityTypes()
											where typeof(BaseEntity).IsAssignableFrom(e.ClrType)
											select e)
		{
			modelBuilder.Entity(item.ClrType).Property("Id").HasDefaultValueSql("uuid_generate_v4()")
				.ValueGeneratedOnAdd();
			modelBuilder.Entity(item.ClrType).Property("CreatedAt").HasDefaultValueSql("now()")
				.ValueGeneratedOnAdd();
		}

		base.OnModelCreating(modelBuilder);
	}

	public static async Task EnsurePostgreSqlExtensionsAsync(DbContext context)
	{
		DbConnection conn = context.Database.GetDbConnection();
		if (conn.State != ConnectionState.Open)
		{
			await conn.OpenAsync();
		}

		using DbCommand cmd = conn.CreateCommand();
		cmd.CommandText = "CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\" SCHEMA public;";
		cmd.CommandType = CommandType.Text;
		await cmd.ExecuteNonQueryAsync();
	}
}
