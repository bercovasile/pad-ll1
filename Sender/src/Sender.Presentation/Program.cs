using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Broker.Grpc;
using Grpc.Core;
using Grpc.Net.Client;

class PublisherApp
{
    static async Task Main(string[] args)
    {
        Console.Write("Select transport (socket/grpc): ");
        var transport = Console.ReadLine()?.ToLower();

        if (transport == "grpc")
        {
            await RunGrpcPublisher();
        }
        else
        {
            await RunSocketPublisher();
        }
    }

    static async Task RunSocketPublisher()
    {
        string host = "127.0.0.1";
        int port = 35000;

        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"[Socket Publisher] Connected to broker at {host}:{port}");

        using var stream = client.GetStream();

        while (true)
        {
            Console.Write("Enter topic:key:value (e.g., test:1:hello): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            var bytes = Encoding.UTF8.GetBytes(input + "\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
            Console.WriteLine($"[Socket Publisher] Sent: {input}");
        }
    }

	//static async Task RunGrpcPublisher()
	//{
	//    using var channel = GrpcChannel.ForAddress("http://localhost:37001");
	//    var client = new BrokerReceiver.BrokerReceiverClient(channel);
	//    using var call = client.StreamMessages();

	//    Console.WriteLine("[gRPC Publisher] Connected to broker");

	//    var readTask = Task.Run(async () =>
	//    {
	//        await foreach (var response in call.ResponseStream.ReadAllAsync())
	//        {
	//            Console.WriteLine($"[Broker ACK] success={response.Success}, message={response.Message}, data={response.Data}");
	//        }
	//    });

	//    while (true)
	//    {
	//        Console.Write("Enter topic:key:value (e.g., test:1:hello): ");
	//        var input = Console.ReadLine();
	//        if (string.IsNullOrWhiteSpace(input)) continue;

	//        var parts = input.Split(':');
	//        if (parts.Length < 3) { Console.WriteLine("Invalid format"); continue; }

	//        var message = new MessageRequest
	//        {
	//            Topic = parts[0],
	//            Key = parts[1],
	//            Value = parts[2],
	//            Headers = { { "SentAt", DateTime.UtcNow.ToString("o") } },
	//            Context = { { "Client", "GrpcPublisher" } },
	//            Priority = 0
	//        };  

	//        await call.RequestStream.WriteAsync(message);
	//        Console.WriteLine($"[gRPC Publisher] Sent: {message.Topic}:{message.Key}:{message.Value}");
	//    }
	//}
	static async Task RunGrpcPublisher()
	{
		using var channel = GrpcChannel.ForAddress("http://localhost:37001");
		var client = new BrokerReceiver.BrokerReceiverClient(channel);

		Console.WriteLine("[gRPC Publisher] Connected to broker");

		while (true)
		{
			Console.Write("Enter topic:key:value (e.g., test:1:hello) or 'exit': ");
			var input = Console.ReadLine();
			if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
				break;

			if (string.IsNullOrWhiteSpace(input)) continue;

			var parts = input.Split(':');
			if (parts.Length < 3)
			{
				Console.WriteLine("Invalid format");
				continue;
			}

			var message = new MessageRequest
			{
				Topic = parts[0],
				Key = parts[1],
				Value = parts[2],
				Headers = { { "SentAt", DateTime.UtcNow.ToString("o") } },
				Context = { { "Client", "GrpcPublisher" } },
				Priority = 0
			};

			try
			{
				// 🔹 deschidem un stream nou pentru fiecare mesaj
				using var call = client.StreamMessages();

				// trimitem mesajul
				await call.RequestStream.WriteAsync(message);

				// închidem fluxul de cereri (important!)
				await call.RequestStream.CompleteAsync();

				// citim toate răspunsurile (ACK-uri)
				await foreach (var response in call.ResponseStream.ReadAllAsync())
				{
					Console.WriteLine($"[Broker ACK] success={response.Success}, message={response.Message}, data={response.Data}");
				}

				Console.WriteLine($"[gRPC Publisher] Sent: {message.Topic}:{message.Key}:{message.Value}");
			}
			catch (RpcException ex)
			{
				Console.WriteLine($"[gRPC Publisher] Stream error: {ex.StatusCode} - {ex.Status.Detail}");
			}
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
