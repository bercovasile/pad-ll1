using Broker.Presentation.Socket.WebSocket.Handlers;

namespace Broker.Presentation.Socket;

public static class EndpointRouteBuilderExtensions
{
	//public static IEndpointRouteBuilder MapSubscriberSocketBroker(
	//	this IEndpointRouteBuilder endpoints,
	//	string pattern)
	//{
	//	endpoints.Map(pattern, async context =>
	//	{
	//		var handler = context.RequestServices.GetRequiredService<WebSocketSubscriberMessageHandler>();
	//		await handler.HandleAsync(context);
	//	});

	//	return endpoints;
	//}

	public static IEndpointRouteBuilder MapReceiverSocketBroker(
		this IEndpointRouteBuilder endpoints,
		string pattern)
	{
		endpoints.Map(pattern, async context =>
		{
			var handler = context.RequestServices.GetRequiredService<WebSocketReceiverMessageHandler>();
			await handler.HandleAsync(context);
		});

		return endpoints;
	}

	public static IEndpointRouteBuilder MapSocketBrokerManagement(
		this IEndpointRouteBuilder endpoints,
		string pattern)
	{
		endpoints.Map(pattern, async context =>
		{
			var handler = context.RequestServices.GetRequiredService<WebSocketManagementMessageHandler>();
			await handler.HandleAsync(context);
		});

		return endpoints;
	}
}