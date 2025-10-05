using Broker.Application.Features.Messages.Commands.Add;
using Broker.Grpc;
using Grpc.Core;
using MediatR;

public class GrpcReceiverMessageHandler
{
    private readonly ISender _sender;

    public GrpcReceiverMessageHandler(ISender sender)
    {
        
        _sender = sender;
    }

    public async Task HandleAsync(
        IAsyncStreamReader<MessageRequest> requestStream,
        IServerStreamWriter<Response> responseStream,
        CancellationToken cancellation = default)
    {

		try
		{
			if (await requestStream.MoveNext(cancellation))
			{
				Console.WriteLine($"Received: {requestStream.Current.Key} = {requestStream.Current.Value}");
				var response = await _sender.Send(
				new AddMessageCommand(
					requestStream.Current.Topic,
					new Broker.Context.Messages.MessageRequest
					{
						Key = requestStream.Current.Key,
						Value = requestStream.Current.Value,
						Headers = requestStream.Current.Headers.ToDictionary(h => h.Key, h => h.Value),
						Priority = requestStream.Current.Priority,
						Context = requestStream.Current.Context.ToDictionary(c => c.Key, c => c.Value),
						TopicName = requestStream.Current.Topic
						}),
					cancellation
				);

				await responseStream.WriteAsync(new Response
				{
					Success = response.Success,
					Message = response.Message,
					Data = string.Empty
				});

				Console.WriteLine($"Message stored: {response.Message}");
			}
		}
		catch (Exception ex)
		{
			await responseStream.WriteAsync(new Response { Message = ex.Message });
			Console.WriteLine($"Error sending message: {ex}");
		}
	}
}
