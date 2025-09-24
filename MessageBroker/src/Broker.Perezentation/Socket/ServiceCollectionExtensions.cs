

using Broker.Application.Abstractions.Receiver;
using Broker.Application.Abstractions.Subscriber;
using Broker.Infrastructure.Receiver;
using Broker.Infrastructure.Subscriber;
using Broker.Presentation.Core.Abstractions.Subscriber;
using Broker.Presentation.Core.Subscriber;
using Broker.Presentation.Socket.Handlers;
using Broker.Presentation.Socket.Receiver;
using Broker.Presentation.Socket.Subscriber;

namespace Broker.Presentation.Socket;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection UseWebSocketReceiverBroker(this IServiceCollection services)
	{
		services.AddSingleton<IWebSocketReceiverBroker, WebSocketReceiverBroker>();
		services.AddSingleton<IMessageReceiverPipeline, MessageReceiverPipeline>();
		services.AddSingleton<WebSocketReceiverMessageHandler>();
		services.AddSingleton<WebSocketManagementMessageHandler>();
		services.AddTransient<IMessageReceiver, WebSocketMessageReceiver>();

		return services;
	}

	public static IServiceCollection UseWebSocketSubscriberBroker(this IServiceCollection services)
	{
		services.AddSingleton<IWebSocketSubscriberBroker, WebSocketSubscriberBroker>();
		services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
		services.AddSingleton<WebSocketSubscriberMessageHandler>();
		services.AddSingleton<IBrokerSender, BrokerSender>();

		return services;
	}
}