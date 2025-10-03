using Broker.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

class SubscriberApp
{
    static async Task Main(string[] args)
    {
        Console.Write("Select transport (socket/grpc): ");
        var transport = Console.ReadLine()?.ToLower();

        if (transport == "grpc")
        {
            await RunGrpcSubscriber();
        }
        else
        {
            await RunSocketSubscriber();
        }
    }

    static async Task RunSocketSubscriber()
    {
        string host = "127.0.0.1";
        int port = 37000;

        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"[Socket Subscriber] Connected to broker");

        using var stream = client.GetStream();

        var topicObj = new { topic = "test_13" };
        string json = JsonSerializer.Serialize(topicObj) + "\n";
        await stream.WriteAsync(Encoding.UTF8.GetBytes(json));
        Console.WriteLine($"[Socket Subscriber] Sent topic: {json}");

        var buffer = new byte[4096];
        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (bytesRead == 0) break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"[Socket Subscriber] Received: {message}");
        }
    }

    static async Task RunGrpcSubscriber()
    {
        using var channel = GrpcChannel.ForAddress("http://localhost:37101");
        var client = new BrokerReceiver.BrokerReceiverClient(channel);

        using var call = client.StreamMessages();

        // Optional: send initial subscription request
        await call.RequestStream.WriteAsync(new MessageRequest
        {
            Topic = "test_13",
            Key = "1",
            Value = "",
            Headers = { { "SubscribedAt", DateTime.UtcNow.ToString("o") } },
            Context = { { "Client", "GrpcSubscriber" } }
        });

        Console.WriteLine("[gRPC Subscriber] Listening for messages...");

        await foreach (var response in call.ResponseStream.ReadAllAsync())
        {
            Console.WriteLine($"[gRPC Subscriber] success={response.Success}, message={response.Message}, data={response.Data}");
        }
    }

    public static MessageRequest ToGrpcMessage(string topic, string key, string value)
    {
        return new MessageRequest
        {
            Topic = topic,
            Key = key,
            Value = value,
            Headers = { { "SentAt", DateTime.UtcNow.ToString("o") } },
            Context = { { "Client", "GrpcPublisherApp" } },
            Priority = 0
        };
    }
}