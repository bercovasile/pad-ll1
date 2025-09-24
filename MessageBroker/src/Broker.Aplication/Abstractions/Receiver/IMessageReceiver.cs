using Broker.Application.Abstractions;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;

namespace Broker.Application.Abstractions.Receiver;

public interface IMessageReceiver
{
	ITopicContext Context { get; }
	Task<T?> ReceiveAsync<T>(CancellationToken cancellation) where T : new();
	IAsyncEnumerable<T?> ReceiveAsyncEnumerable<T>(
	[EnumeratorCancellation] CancellationToken cancellation = default) where T : new();
	Task AckAsync<T>(T message, CancellationToken cancellation);
	Task NackAsync<T>(T message, CancellationToken cancellation);
}