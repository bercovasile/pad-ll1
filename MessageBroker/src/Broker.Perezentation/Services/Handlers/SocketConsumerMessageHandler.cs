// Broker.Perezentation\Services\Handlers\SocketConsumerMessageHandler.cs
using Broker.Infrastructure.Services;
using System.Net.Sockets;
using System.Threading;

namespace Broker.Presentation.Services.Handlers;

public class SocketConsumerMessageHandler
{
    private readonly BrokerConnection _brokerConnection;

    public SocketConsumerMessageHandler(BrokerConnection brokerConnection)
    {
        _brokerConnection = brokerConnection;
    }

    /// <summary>
    /// Acceptă și procesează un consumator socket pentru un topic.
    /// </summary>
    public async Task HandleAsync(System.Net.Sockets.Socket socket, string topic, CancellationToken cancellation = default)
    {
        var consumer = await _brokerConnection.AcceptSocketConsumerAsync(socket, topic, cancellation);
        if (consumer == null)
        {
            Console.WriteLine("Socket consumer could not be accepted.");
            return;
        }

        Console.WriteLine($"Socket consumer accepted for topic: {topic}");

        consumer.Acks.Subscribe(ack =>
        {
            Console.WriteLine($"ACK received for message {ack.MessageId}: {ack.Type} - {ack.Reason}");
        });

        // Poți adăuga logica suplimentară pentru trimiterea de mesaje către consumator
    }
}