using Broker.Application.Abstractions.Receiver;
using Broker.Application.Features.Messages.Commands.Add;
using System.Net.Sockets;
using Broker.Context.Messages;
using MediatR;

namespace Broker.Presentation.Servicse.Handlers;

public class SocketReceiverMessageHandler
{
	private readonly ISocketReceiverBroker _broker;
	private readonly IMessageReceiverPipeline _pipeline;
	private readonly ISender _sender;

	public SocketReceiverMessageHandler(ISocketReceiverBroker broker, IMessageReceiverPipeline pipeline, ISender sender)
	{
		_broker = broker;
		_pipeline = pipeline;
		_sender = sender;
	}

	/// <summary>
	/// Handles incoming socket messages using the receiver pipeline.
	/// </summary>
	/// <param name="socket">The connected socket.</param>
	/// <param name="cancellation">Cancellation token.</param>
	public async Task HandleAsync(System.Net.Sockets.Socket socket, CancellationToken cancellation = default)
	{
		var receiver = await _broker.AcceptReceiverAsync(socket, cancellation);
		if (receiver == null)
		{
			Console.WriteLine("Socket connection could not be accepted.");
			return;
		}

		await _pipeline.RunAsync<MessageRequest>(
			receiver,
			async msg =>
			{
				Console.WriteLine($"Received: {msg}");
				try
				{
					var response = await _sender.Send(new AddMessageCommand(receiver.Context.ContextTopic, msg), cancellation);
					Console.WriteLine($"Message stored: {response}");
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error sending message: {ex}");
					throw;
				}
			},
			onSuccess: async msg => Console.WriteLine($"Processed OK: {msg}"),
			onFailure: async msg => Console.WriteLine($"Failed: {msg}"),
			cancellation: cancellation
		);
	}
}
