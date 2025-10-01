using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class PublisherApp
{
    static async Task Main(string[] args)
    {
        string host = "10.48.48.65";
        int port = 35000;

        using var client = new TcpClient();
        await client.ConnectAsync(host, port);
        Console.WriteLine($"[Publisher] Connected to broker at {host}:{port}");

        using var stream = client.GetStream();

        while (true)
        {
            Console.Write("Enter message: ");
            var message = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(message)) continue;

            var bytes = Encoding.UTF8.GetBytes(message + "\n"); // add delimiter if server expects lines
            await stream.WriteAsync(bytes, 0, bytes.Length);

            Console.WriteLine($"[Publisher] Sent: {message}");
        }
    }
}
