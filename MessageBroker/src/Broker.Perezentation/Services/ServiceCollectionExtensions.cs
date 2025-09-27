

using Broker.Application.Abstractions.Receiver;
using Broker.Infrastructure.Receiver;
using Broker.Infrastructure.Receiver.Socket;
using Broker.Infrastructure.Receiver.Web;

using Broker.Presentation.Services.Handlers;
using Broker.Presentation.Servicse.Handlers;
using Broker.Presentation.Socket.WebSocket.Handlers;

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
			var logger = provider.GetRequiredService<ILogger<SocketServerHostedService>>();
			return new SocketServerHostedService(handler,logger, port: 35000);
		});


		return services;
	}

	public static IServiceCollection UseWebSocketSubscriberBroker(this IServiceCollection services)
	{
		//services.AddSingleton<IWebSocketSubscriberBroker, WebSocketSubscriberBroker>();
		//services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
		//services.AddSingleton<WebSocketSubscriberMessageHandler>();
		//services.AddSingleton<IBrokerSender, BrokerSender>();

		return services;
	}
}