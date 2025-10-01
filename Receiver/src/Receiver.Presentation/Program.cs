using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class SubscriberApp
{
    static async Task Main(string[] args)
    {
        string topic = "news";
        string uri = "ws://localhost:51800/messages/subscriber/news";

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(uri), CancellationToken.None);
        Console.WriteLine($"[Subscriber] Connected to broker on topic '{topic}'");

        var buffer = new byte[1024 * 4];
        while (true)
        {
            var result = await ws.ReceiveAsync(buffer, CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine("[Subscriber] Connection closed by broker.");
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"[Subscriber] Received: {message}");
        }
    }
}