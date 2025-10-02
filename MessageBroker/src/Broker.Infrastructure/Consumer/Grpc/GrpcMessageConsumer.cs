using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Broker.Application.Abstractions.Consumer;
using Broker.Domain.Entites.Messages;
using Broker.Context.Response;
using Broker.Domain.Entites.Consumer;

namespace Broker.Infrastructure.Consumer.Grpc
{
    public class GrpcMessageConsumer : IMessageConsumer
    {
        public string ConsumerId { get; }
        public string Topic { get; }
        public IObservable<MessageAcknowledgment> Acks => _ackSubject.AsObservable();

        private readonly Subject<MessageAcknowledgment> _ackSubject = new();
        private readonly IAsyncStreamReader<object>? _clientStream;
        private readonly IServerStreamWriter<object> _serverStream;
        private CancellationTokenSource _cts = new();
        private bool _disposed = false;
        private readonly Task? _ackReaderTask;

        public GrpcMessageConsumer(string consumerId, string topic,
            IAsyncStreamReader<object>? clientStream,
            IServerStreamWriter<object> serverStream,
            CancellationToken? parentCancellation = null)
        {
            ConsumerId = consumerId;
            Topic = topic;
            _clientStream = clientStream;
            _serverStream = serverStream;

            if (parentCancellation != null)
            {
                _cts = CancellationTokenSource.CreateLinkedTokenSource(parentCancellation.Value);
            }

            if (_clientStream != null)
            {
                _ackReaderTask = Task.Run(() => ReadClientLoopAsync(_cts.Token));
            }
        }

        public async Task<Response> ConsumeAsync(Message message, CancellationToken cancellation)
        {
            if (_disposed) return new Response { Success = false, Message = "Consumer disposed" };

            try
            {
                var payload = new Dictionary<string, object?>
                {
                    ["id"] = message.Id.ToString(),
                    ["key"] = message.Key,
                    ["value"] = message.Value,
                    ["headers"] = message.Headers ?? new Dictionary<string,string>(),
                    ["priority"] = message.Priority,
                    ["timestamp"] = message.Timestamp.ToString("o"),
                    ["topic"] = message.TopicId.ToString()
                };

                await _serverStream.WriteAsync(payload).ConfigureAwait(false);
                return new Response { Success = true, Message = "Sent" };
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.Cancelled || rpcEx.StatusCode == StatusCode.Unavailable)
            {
                return new Response { Success = false, Message = $"gRPC stream closed: {rpcEx.Message}" };
            }
            catch (Exception ex)
            {
                return new Response { Success = false, Message = ex.Message };
            }
        }

        private async Task ReadClientLoopAsync(CancellationToken cancellation)
        {
            try
            {
                while (!cancellation.IsCancellationRequested && _clientStream != null && await _clientStream.MoveNext(cancellation).ConfigureAwait(false))
                {
                    var obj = _clientStream.Current;
                    if (obj == null) continue;

                    try
                    {
                        // Try dictionary-like
                        string? messageIdStr = null;
                        string? typeStr = null;
                        string? reason = null;

                        if (obj is IDictionary dict)
                        {
                            if (dict.Contains("messageId")) messageIdStr = dict["messageId"]?.ToString();
                            if (dict.Contains("type")) typeStr = dict["type"]?.ToString();
                            if (dict.Contains("reason")) reason = dict["reason"]?.ToString();
                        }
                        else
                        {
                            // Try reflection-based property access
                            var t = obj.GetType();
                            var pid = t.GetProperty("MessageId") ?? t.GetProperty("messageId") ?? t.GetProperty("Id") ?? t.GetProperty("id");
                            var ptype = t.GetProperty("Type") ?? t.GetProperty("type") ?? t.GetProperty("AckType");
                            var preason = t.GetProperty("Reason") ?? t.GetProperty("reason");

                            if (pid != null) messageIdStr = pid.GetValue(obj)?.ToString();
                            if (ptype != null) typeStr = ptype.GetValue(obj)?.ToString();
                            if (preason != null) reason = preason.GetValue(obj)?.ToString();
                        }

                        if (!string.IsNullOrWhiteSpace(messageIdStr) && Guid.TryParse(messageIdStr, out var gid))
                        {
                            var ackType = AckType.Nak;
                            if (!string.IsNullOrWhiteSpace(typeStr) && typeStr.Equals("Ack", StringComparison.OrdinalIgnoreCase)) ackType = AckType.Ack;

                            var ack = new MessageAcknowledgment { MessageId = gid, Type = ackType, Reason = reason };
                            _ackSubject.OnNext(ack);
                        }
                    }
                    catch
                    {
                        // ignore malformed ack
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (RpcException) { }
            catch (Exception ex)
            {
                _ackSubject.OnError(ex);
            }
            finally
            {
                _ackSubject.OnCompleted();
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            try { _cts.Cancel(); } catch { }
            _ackSubject.Dispose();
        }
    }
}
