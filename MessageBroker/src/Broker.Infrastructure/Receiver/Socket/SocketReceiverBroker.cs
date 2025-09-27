

namespace Broker.Infrastructure.Receiver.Socket;

using Broker.Application.Abstractions.Receiver;
using Broker.Infrastructure.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

public class SocketReceiverBroker : ISocketReceiverBroker
{
	private readonly IServiceProvider _provider;

	public SocketReceiverBroker(IServiceProvider provider)
	{
		_provider = provider;
	}

	public async Task<IBrokerReceiver?> AcceptReceiverAsync(Socket socket, CancellationToken cancellation = default)
	{
		if (socket == null || !socket.Connected)
			return null;

		var buffer = new byte[1024];
		int received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellation);

		if (received == 0)
			return null;

		var json = Encoding.UTF8.GetString(buffer, 0, received);
		var topic = "defaultTopic";

		try
		{
			using var doc = JsonDocument.Parse(json);
			if (doc.RootElement.TryGetProperty("topic", out var t))
			{
				topic = t.GetString() ?? topic;
			}
		}
		catch
		{
			// 
		}

		var receiver = _provider.GetRequiredService<IBrokerReceiver>();
		if (receiver is SocketMessageReceiver socketReceiver)
		{
			socketReceiver.SetSocket(socket);
			socketReceiver.SetTopic(new TopicContext(topic));
		}

		return receiver;
	}

}
