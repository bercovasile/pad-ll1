using Broker.Application.Abstractions.Receiver;
using Broker.Application.Features.Messages.Commands.Add;
using Broker.Context.Messages;
using Broker.Context.Response;
using Broker.Infrastructure.Receiver.grpc;
using Grpc.Core;
using MediatR;

namespace Broker.Presentation.Services.gRPC.Handlers;

public class GrpcReceiverMessageHandler
{
	private readonly IMessageReceiverPipeline _pipeline;
	private readonly ISender _sender;

	public GrpcReceiverMessageHandler(IMessageReceiverPipeline pipeline, ISender sender)
	{
		_pipeline = pipeline;
		_sender = sender;
	}


	public async Task HandleAsync(
		IAsyncStreamReader<MessageRequest> requestStream,
		IServerStreamWriter<Response> responseStream,
		CancellationToken cancellation = default)
	{
		var receiver = new GrpcMessageReceiver<MessageRequest, Response>(requestStream, responseStream);

		await _pipeline.RunAsync<MessageRequest>(
			receiver,
			async msg =>
			{
				Console.WriteLine($"Received: {msg.Key} = {msg.Value}");
				try
				{
					var response = await _sender.Send(new AddMessageCommand(msg.TopicName, msg), cancellation);

					// Trimitem ACK înapoi către client
					await responseStream.WriteAsync(response);
					Console.WriteLine($"Message stored: {response.Message}");
				}
				catch (Exception ex)
				{
					await responseStream.WriteAsync(new Response { Message = ex.Message });

					Console.WriteLine($"Error sending message: {ex}");
					throw;
				}
			},
			onSuccess: async msg => Console.WriteLine($"Processed OK: {msg.Key}"),
			onFailure: async msg => Console.WriteLine($"Failed: {msg.Key}"),
			cancellation: cancellation
		);
	}
}