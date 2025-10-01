using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class TopicMessage
{
    public string topic { get; set; }
}

class SubscriberApp
{
    static async Task Main(string[] args)
    {
		string host = "127.0.0.1";

		int port = 37000;

        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"[Subscriber] Connected to broker");

        using var stream = client.GetStream();

        // Create the topic object
        var topicMessage = new TopicMessage { topic = "test_13" };

        // Serialize to JSON
        string json = JsonSerializer.Serialize(topicMessage);
        byte[] topicBytes = Encoding.UTF8.GetBytes(json + "\n"); // newline if server expects line-based
        await stream.WriteAsync(topicBytes, 0, topicBytes.Length);
        await stream.FlushAsync();
        Console.WriteLine($"[Subscriber] Sent topic: {json}");

        var buffer = new byte[4096];
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0)
            {
                Console.WriteLine("[Subscriber] Connection closed by broker.");
                break;
            }

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[Subscriber] Received: {message}");
        }
    }
}