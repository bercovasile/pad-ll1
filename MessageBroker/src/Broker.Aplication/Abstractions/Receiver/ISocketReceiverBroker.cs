

using System.Net.Sockets;

namespace Broker.Application.Abstractions.Receiver;

public interface ISocketReceiverBroker
{
	Task<IBrokerReceiver?> AcceptReceiverAsync(Socket socket, CancellationToken cancellation = default);
}