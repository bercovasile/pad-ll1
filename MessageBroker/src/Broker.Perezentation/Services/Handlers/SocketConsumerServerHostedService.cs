// Broker.Perezentation\Services\Handlers\SocketConsumerServerHostedService.cs
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Broker.Presentation.Services.Handlers;

public class SocketConsumerServerHostedService : BackgroundService
{
    private readonly SocketConsumerMessageHandler _handler;
    private readonly ILogger<SocketConsumerServerHostedService> _logger;
    private readonly Channel<System.Net.Sockets.Socket> _connectionChannel;
    private readonly ChannelWriter<System.Net.Sockets.Socket> _connectionWriter;
    private readonly ChannelReader<System.Net.Sockets.Socket> _connectionReader;
    private readonly int _port;
    private readonly int _maxConcurrentConnections;
    private readonly int _maxPendingConnections;
    private TcpListener? _listener;
    private readonly ConcurrentBag<Task> _workerTasks = new();
    private readonly SemaphoreSlim _connectionSemaphore;

    public SocketConsumerServerHostedService(
        SocketConsumerMessageHandler handler,
        ILogger<SocketConsumerServerHostedService> logger,
        int port = 6000,
        int maxConcurrentConnections = 100,
        int maxPendingConnections = 1000)
    {
        _handler = handler;
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
                await _handler.HandleAsync(clientSocket, "defaultTopic", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling consumer socket connection from {RemoteEndPoint}", clientSocket.RemoteEndPoint);
            }
            finally
            {
                try
                {
                    clientSocket.Close();
                    _logger.LogDebug("Consumer connection closed for {RemoteEndPoint}", clientSocket.RemoteEndPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error closing consumer socket");
                }
                _connectionSemaphore.Release();
            }
        }
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