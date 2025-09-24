using Broker.Application.Abstractions.Receiver;
using Broker.Application.Features.Topics.Commands.Add;
using Broker.Context.Topics.Requests;
using MediatR;

namespace Broker.Presentation.Socket.Handlers;

public class WebSocketManagementMessageHandler
{
	private readonly IWebSocketReceiverBroker _broker;
	private readonly IMessageReceiverPipeline _pipeline;
	private readonly ISender _sender;
	public WebSocketManagementMessageHandler(IWebSocketReceiverBroker broker, IMessageReceiverPipeline pipeline, ISender sender)
	{
		_broker = broker;
		_pipeline = pipeline;
		_sender = sender;	
	}

	public async Task HandleAsync(HttpContext context, CancellationToken cancellation = default)
	{
		var receiver = await _broker.AcceptReceiverAsync(context, cancellation);
		if (receiver == null)
		{
			context.Response.StatusCode = 400;
			return;
		}

		await _pipeline.RunAsync<TopicRequest>(
			receiver,
			async msg =>
			{
				Console.WriteLine($"Received: {msg}");

				try
				{

					var response = await _sender.Send(new AddTopicCommand(receiver.Context.ContextTopic, msg), cancellation);
					Console.WriteLine($"Topic created: {response}");
				}catch(Exception ex)
				{
					throw;
				}

			},
			onSuccess: async msg => Console.WriteLine($"Processed OK: {msg}"),
			onFailure: async msg => Console.WriteLine($"Failed: {msg}"),
			cancellation: cancellation
		);
	}
}
