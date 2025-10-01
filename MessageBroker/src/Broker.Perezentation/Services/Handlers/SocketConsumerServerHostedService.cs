// Broker.Perezentation\Services\Handlers\SocketConsumerServerHostedService.cs
using Broker.Application.Abstractions;
using Broker.Context.Response;
using Broker.Infrastructure.Services;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Broker.Presentation.Services.Handlers;

public class SocketConsumerServerHostedService : BackgroundService
{
    private readonly ILogger<SocketConsumerServerHostedService> _logger;
    private readonly Channel<System.Net.Sockets.Socket> _connectionChannel;
    private readonly ChannelWriter<System.Net.Sockets.Socket> _connectionWriter;
    private readonly ChannelReader<System.Net.Sockets.Socket> _connectionReader;
	private readonly BrokerConnection _brokerConnection;
	private readonly int _port;
    private readonly int _maxConcurrentConnections;
    private readonly int _maxPendingConnections;
    private TcpListener? _listener;
    private readonly ConcurrentBag<Task> _workerTasks = new();
    private readonly SemaphoreSlim _connectionSemaphore;
    private readonly IBaseTopicProvider _baseTopicProvide;



	public SocketConsumerServerHostedService(
		BrokerConnection brokerConnection,
		IBaseTopicProvider baseTopicProvide,
		ILogger<SocketConsumerServerHostedService> logger,
		int port = 6000,
        int maxConcurrentConnections = 100,
        int maxPendingConnections = 1000
		)
    {
        _logger = logger;
        _port = port;
        _maxConcurrentConnections = maxConcurrentConnections;
        _maxPendingConnections = maxPendingConnections;
        _connectionSemaphore = new SemaphoreSlim(maxConcurrentConnections, maxConcurrentConnections);

        var options = new BoundedChannelOptions(maxPendingConnections)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = false,
            SingleWriter = true
        };
        _connectionChannel = Channel.CreateBounded<System.Net.Sockets.Socket>(options);
        _connectionWriter = _connectionChannel.Writer;
        _connectionReader = _connectionChannel.Reader;
        _brokerConnection = brokerConnection;
        _baseTopicProvide = baseTopicProvide;

	}

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start(_maxPendingConnections);
        _logger.LogInformation("Socket consumer server started on port {Port} with backlog {Backlog}", _port, _maxPendingConnections);

        for (int i = 0; i < _maxConcurrentConnections; i++)
        {
            var workerTask = ProcessConnectionsAsync(stoppingToken);
            _workerTasks.Add(workerTask);
        }

        await AcceptConnectionsAsync(stoppingToken);
        await Task.WhenAll(_workerTasks);
    }

    private async Task AcceptConnectionsAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var clientSocket = await _listener.AcceptSocketAsync(stoppingToken);
                    if (!clientSocket.Connected)
                    {
                        clientSocket.Close();
                        continue;
                    }
                    _logger.LogDebug("New consumer connection accepted from {RemoteEndPoint}", clientSocket.RemoteEndPoint);
                    await _connectionWriter.WriteAsync(clientSocket, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error accepting consumer socket connection");
                }
            }
        }
        finally
        {
            _connectionWriter.TryComplete();
            _logger.LogInformation("Consumer connection acceptance stopped");
        }
    }

    private async Task ProcessConnectionsAsync(CancellationToken stoppingToken)
    {
        await foreach (var clientSocket in _connectionReader.ReadAllAsync(stoppingToken))
        {
            await _connectionSemaphore.WaitAsync(stoppingToken);
            try
            {
                _logger.LogDebug("Processing consumer connection from {RemoteEndPoint}", clientSocket.RemoteEndPoint);
                var topicName = await ReceiveTopicAsync(clientSocket, stoppingToken);

				var topic = await _baseTopicProvide.GetTopicAsync(topicName ?? "default", stoppingToken);
				//if (topic == null)
				//	return Response.Fail($"Topic '{topicName}' don't exists.");

				await _brokerConnection.AcceptSocketConsumerAsync(clientSocket, topic.Id.ToString(), stoppingToken);

			}
			catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling consumer socket connection from {RemoteEndPoint}", clientSocket.RemoteEndPoint);
                try
                {
                    _logger.LogDebug("Consumer connection closed for {RemoteEndPoint}", clientSocket.RemoteEndPoint);
                    clientSocket.Close();
                }
                catch (Exception exi)
                {
                    _logger.LogWarning(exi, "Error closing consumer socket");
                }
                _connectionSemaphore.Release();
            }
         
        }
    }

	public async Task<string?> ReceiveTopicAsync(System.Net.Sockets.Socket socket, CancellationToken cancellation = default)
	{
		if (socket == null || !socket.Connected)
			return null;

		var buffer = new byte[1024];
		int received = await socket.ReceiveAsync(buffer, SocketFlags.None, cancellation);

		if (received == 0)
			return null;

		var json = Encoding.UTF8.GetString(buffer, 0, received);

		try
		{
			using var doc = JsonDocument.Parse(json);
			if (doc.RootElement.TryGetProperty("topic", out var topicElement))
			{
				return topicElement.ToString(); // Clone pentru a nu fi legat de viața lui JsonDocument
			}
		}
		catch
		{
			// dacă JSON e invalid -> null
		}

		return null;
	}


	public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _listener?.Stop();
        _connectionWriter.TryComplete();
        _logger.LogInformation("Socket consumer server stopped");
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _listener?.Stop();
        _connectionSemaphore?.Dispose();
        base.Dispose();
    }
}