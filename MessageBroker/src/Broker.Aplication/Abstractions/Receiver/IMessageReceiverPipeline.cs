namespace Broker.Application.Abstractions.Receiver;

public interface IMessageReceiverPipeline
{
	Task RunAsync<T>(
		IMessageReceiver consumer,
		Func<T, Task> onDataReceived,
		Func<T, Task>? onSuccess = null,
		Func<T, Task>? onFailure = null,
		Func<Exception, Task>? onException = null,
		int maxDegreeOfParallelism = 1,
		CancellationToken cancellation = default) where T : new();
	Task RunAndAckAsync<T>(
		IMessageReceiver consumer,
		Func<T, Task<bool>> onDataReceived,
		Func<T, Task>? onSuccess = null,
		Func<T, Task>? onFailure = null,
		Func<Exception, Task>? onException = null,
		int maxDegreeOfParallelism = 1,
		CancellationToken cancellation = default) where T : new();

}
