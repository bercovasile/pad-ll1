using Broker.Application.Abstractions;
using Broker.Domain.Abstractions;
using Broker.Infrastructure.Services;
using Broker.Persistence.Config;
using Broker.Persistence.Contexts;
using Broker.Persistence.Contexts.Postgress;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Broker.Persistence;

public static class ServiceCollectionExtensions
{

	public static IServiceCollection AddTopicProviders(
		this IServiceCollection services)
	{
		services.AddSingleton<ITopicProvider, TopicProviderLogBased>();
		services.AddSingleton<ITopicProvider, TopicProviderQueueBased>();
		services.AddSingleton<ITopicProviderFactory, TopicProviderFactory>();
		services.AddSingleton<IBaseTopicProvider, BaseTopicProvider>();

		return services;
	}
	public static IServiceCollection AddMongoBroker(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		// Bind MongoDbSettings from configuration section
		var settings = new MongoDbSettings();
		configuration.GetSection("MongoDb").Bind(settings);

		// Validate settings
		settings.Validate();


		services.AddSingleton(settings);
		services.AddSingleton<IMongoBrokerContextFactory, MongoBrokerContextFactory>();
		
		return services;
	}
	public static IServiceCollection AddPostgreSQLBroker(
		this IServiceCollection services, IConfiguration configuration)
	{
		services.AddPooledDbContextFactory<BrokerPostgresContext>((serviceProvider, options) =>
		{
			var configuration = serviceProvider.GetRequiredService<IConfiguration>();

			options.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection"),
				npgsqlOptions => npgsqlOptions.EnableRetryOnFailure() // optional
			);
			Npgsql.NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
		});

		return services;
	}


}