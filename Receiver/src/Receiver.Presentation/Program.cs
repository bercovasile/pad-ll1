//using Broker.Grpc;
using Grpc.Core;
using Grpc.Net.Client;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Broker.Presentation.Protos.Consumer;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

class SubscriberApp
{
    static async Task Main(string[] args)
    {
        Console.Write("Select transport (socket/grpc): ");
        var transport = Console.ReadLine()?.ToLower();

        if (transport == "grpc")
        {
			 await RunSubscriberAsServerAsync("test_13");
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

	static async Task RunSubscriberAsServerAsync(string topic)
	{
		var builder = WebApplication.CreateBuilder();
		builder.WebHost.ConfigureKestrel(options =>
		{
			options.ListenLocalhost(51800, o => o.Protocols = HttpProtocols.Http2);
		});

		builder.Services.AddGrpc();
		var app = builder.Build();
		app.MapGrpcService<NotificationReceiverService>();
		_ = app.RunAsync();



		using var channel = GrpcChannel.ForAddress("http://localhost:37101");
		var client = new BrokerConsumer.BrokerConsumerClient(channel);

		using var call = client.Subscribe();

		string localAddress = "http://localhost:51800";

		await call.RequestStream.WriteAsync(new SubscribeRequest
		{
			Topic = topic,
			Address = localAddress
		});

		Console.WriteLine($"[Client] Subscribed to topic '{topic}' and listening on {localAddress}");


		_ = Task.Run(async () =>
		{
			await foreach (var response in call.ResponseStream.ReadAllAsync())
			{
				Console.WriteLine($"[Message] success={response.Success}, message={response.Message}, data={response.Data}");
			}
		});


		await Task.Delay(-1);
	}


}







