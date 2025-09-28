// Broker.Perezentation\Services\WebSocket\Handlers\WebSocketConsumerMessageHandler.cs
using Broker.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Broker.Presentation.Socket.WebSocket.Handlers;

public class WebSocketConsumerMessageHandler
{
    private readonly BrokerConnection _brokerConnection;

    public WebSocketConsumerMessageHandler(BrokerConnection brokerConnection)
    {
        _brokerConnection = brokerConnection;
    }

    /// <summary>
    /// Accept? ?i proceseaz? un consumator WebSocket pentru un topic.
    /// </summary>
    public async Task HandleAsync(HttpContext context, string topic, CancellationToken cancellation = default)
    {
        var consumer = await _brokerConnection.AcceptWebSocketConsumerAsync(context, topic, cancellation);
        if (consumer == null)
        {
            context.Response.StatusCode = 400;
            return;
        }

        Console.WriteLine($"WebSocket consumer accepted for topic: {topic}");

        consumer.Acks.Subscribe(ack =>
        {
            Console.WriteLine($"ACK received for message {ack.MessageId}: {ack.Type} - {ack.Reason}");
        });

        // Po?i ad?uga logica suplimentar? pentru trimiterea de mesaje c?tre consumator
    }
}