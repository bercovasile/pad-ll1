using Broker.Application.Abstractions.Receiver;
using Broker.Application.Features.Messages.Commands.Add;
using Broker.Grpc;
using Broker.Infrastructure.Receiver.grpc;
using Grpc.Core;
using MediatR;

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
        var receiver = new GrpcMessageReceiver<MessageRequest, Response>(requestStream, responseStream, "topic_13");

        await _pipeline.RunAsync<MessageRequest>(
            receiver,
            async msg =>
            {
                Console.WriteLine($"Received: {msg.Key} = {msg.Value}");
                try
                {
                    var response = await _sender.Send(
                        new AddMessageCommand(
                            msg.Topic,
                            new Broker.Context.Messages.MessageRequest
                            {
                                Key = msg.Key,
                                Value = msg.Value,
                                Headers = msg.Headers.ToDictionary(h => h.Key, h => h.Value),
                                Priority = msg.Priority,
                                Context = msg.Context.ToDictionary(c => c.Key, c => c.Value),
                                TopicName = msg.Topic
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
                catch (Exception ex)
                {
                    await responseStream.WriteAsync(new Response { Message = ex.Message });
                    Console.WriteLine($"Error sending message: {ex}");
                }
            },
            onSuccess: async msg => Console.WriteLine($"Processed OK: {msg.Key}"),
            onFailure: async msg => Console.WriteLine($"Failed: {msg.Key}"),
            cancellation: cancellation
        );
    }
}
