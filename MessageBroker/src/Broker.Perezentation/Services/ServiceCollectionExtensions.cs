using Broker.Application;
using Broker.Application.Abstractions.Consumer;
using Broker.Application.Abstractions.Dispatcher;
using Broker.Application.Abstractions.Receiver;
using Broker.Infrastructure.Consumer;
using Broker.Infrastructure.Consumer.Core;
using Broker.Infrastructure.Consumer.Sockets;
using Broker.Infrastructure.Dispatcher;
using Broker.Infrastructure.Jobs;
using Broker.Infrastructure.Receiver;
using Broker.Infrastructure.Receiver.Socket;
using Broker.Infrastructure.Receiver.Web;
using Broker.Infrastructure.Services;
using Broker.Persistence;
using Broker.Presentation.Services.Handlers;
using Broker.Presentation.Servicse.Handlers;
using Broker.Presentation.Socket.WebSocket.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.AspNetCore;

namespace Broker.Presentation.Socket;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection UseWebSocketReceiverBroker(this IServiceCollection services)
	{
		services.AddSingleton<IWebSocketReceiverBroker, WebSocketReceiverBroker>();
		services.AddSingleton<IMessageReceiverPipeline, MessageReceiverPipeline>();
		services.AddSingleton<WebSocketReceiverMessageHandler>();
		services.AddSingleton<WebSocketManagementMessageHandler>();
		services.AddTransient<IBrokerReceiver, WebSocketMessageReceiver>();

		return services;
	}

	public static IServiceCollection UseSocketReceiverBroker(this IServiceCollection services)
	{
		services.AddSingleton<ISocketReceiverBroker, SocketReceiverBroker>();
		services.AddSingleton<IMessageReceiverPipeline, MessageReceiverPipeline>();
		services.AddSingleton<SocketReceiverMessageHandler>();
		services.AddTransient<IBrokerReceiver, SocketMessageReceiver>();
		services.AddHostedService(provider =>
		{
			var handler = provider.GetRequiredService<SocketReceiverMessageHandler>();
			var logger = provider.GetRequiredService<ILogger<SocketReceiverServerHostedService>>();
			return new SocketReceiverServerHostedService(handler,logger, port: 35000);
		});


		return services;
	}

	public static IServiceCollection UseSocketConsumerBroker(this IServiceCollection services)
	{
		//services.AddSingleton<IMessageConsumer, SocketMessageConsumer>();

		services.AddSingleton<IConsumerManager, ConsumerManager>();
		services.AddSingleton<IMessageDispatcher, RoundRobinMessageDispatcher>();
		services.AddSingleton<SocketConsumerMessageHandler>();

		services.AddHostedService(provider =>
		{
			var handler = provider.GetRequiredService<SocketConsumerMessageHandler>();
			var logger = provider.GetRequiredService<ILogger<SocketConsumerServerHostedService>>();
			return new SocketConsumerServerHostedService(handler, logger, port: 37000);
		});

		return services;
	}

	// Adaugă o metodă centralizată pentru înregistrarea tuturor serviciilor necesare aplicației
	public static IServiceCollection AddBrokerServices(this IServiceCollection services, IConfiguration configuration)
	{
		// Memory cache
		services.AddMemoryCache();
		// Persistence
		services.AddMongoBroker(configuration);
		services.AddPostgreSQLBroker(configuration);
		// Topic providers
		services.AddTopicProviders();
		// Application services (MediatR etc.)
		services.AddApplicationServices();
		// Receiver brokers
		services.UseWebSocketReceiverBroker();
		services.UseSocketReceiverBroker();

		services.UseSocketConsumerBroker();

		// Broker connection (shared manager for consumers)
		services.AddSingleton<BrokerConnection>();	

		// Quartz jobs
		services.AddQuartz(q =>
		{
			q.UseMicrosoftDependencyInjectionJobFactory();

			// LogType job
			var logJobKey = new JobKey("LogTypeMessageDispatcherJob");
			q.AddJob<LogTypeMessageDispatcherJob>(opts => opts.WithIdentity(logJobKey));
			q.AddTrigger(opts => opts
				.ForJob(logJobKey)
				.WithIdentity("LogTypeMessageDispatcherJob-trigger")
				.WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));

			// QueueType job
			var queueJobKey = new JobKey("QueueTypeMessageDispatcherJob");
			q.AddJob<QueueTypeMessageDispatcherJob>(opts => opts.WithIdentity(queueJobKey));
			q.AddTrigger(opts => opts
				.ForJob(queueJobKey)
				.WithIdentity("QueueTypeMessageDispatcherJob-trigger")
				.WithSimpleSchedule(x => x.WithIntervalInSeconds(30).RepeatForever()));
		});
		services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

		return services;
	}

}