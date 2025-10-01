using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class PublisherApp
{
    static async Task Main(string[] args)
    {
        string topic = "news";
        string uri = "ws://localhost:51800/messages/publisher/news";

        using var ws = new ClientWebSocket();
        await ws.ConnectAsync(new Uri(uri), CancellationToken.None);
        Console.WriteLine($"[Publisher] Connected to broker on topic '{topic}'");

        while (true)
        {
            Console.Write("Enter message: ");
            var message = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(message)) continue;

            var bytes = Encoding.UTF8.GetBytes(message);
            await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);

            Console.WriteLine($"[Publisher] Sent: {message}");
        }
    }
}
