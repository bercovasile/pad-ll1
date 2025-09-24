using Broker.Application.Abstractions.Receiver;

namespace Broker.Infrastructure.Receiver;

public class MessageReceiverPipeline : IMessageReceiverPipeline
{
	public async Task RunAsync<T>(
		IMessageReceiver consumer,
		Func<T, Task> onDataReceived,
		Func<T, Task>? onSuccess = null,
		Func<T, Task>? onFailure = null,
		Func<Exception, Task>? onException = null,
		int maxDegreeOfParallelism = 1,
		CancellationToken cancellation = default) where T : new()
	{
		var throttler = new SemaphoreSlim(maxDegreeOfParallelism);

		await foreach (var msg in consumer.ReceiveAsyncEnumerable<T>(cancellation))
		{
			if (msg == null) continue;

			await throttler.WaitAsync(cancellation);
			_ = Task.Run(async () =>
			{
				try
				{
					await onDataReceived(msg);
					if (onSuccess != null) await onSuccess(msg);
				}
				catch (Exception ex)
				{
					if (onFailure != null) await onFailure(msg);
					if (onException != null) await onException(ex);
				}
				finally
				{
					throttler.Release();
				}
			}, cancellation);
		}

	}

	public async Task RunAndAckAsync<T>(
			IMessageReceiver consumer,
			Func<T, Task<bool>> onDataReceived,
			Func<T, Task>? onSuccess = null,
			Func<T, Task>? onFailure = null,
			Func<Exception, Task>? onException = null,
			int maxDegreeOfParallelism = 1,
			CancellationToken cancellation = default) where T : new()
	{
		var throttler = new SemaphoreSlim(maxDegreeOfParallelism);

		await foreach (var msg in consumer.ReceiveAsyncEnumerable<T>(cancellation))
		{
			if (msg is null)
				continue;

			await throttler.WaitAsync(cancellation);

			_ = Task.Run(async () =>
			{
				try
				{
					var success = await onDataReceived(msg);
					if (success)
					{
						await consumer.AckAsync(msg, cancellation);
						if (onSuccess is not null) await onSuccess(msg);
					}
					else
					{
						await consumer.NackAsync(msg, cancellation);
						if (onFailure is not null) await onFailure(msg);
					}
				}
				catch (Exception ex)
				{
					if (onException is not null) await onException(ex);
				}
				finally
				{
					throttler.Release();
				}
			}, cancellation);
		}
		;


	}
}


