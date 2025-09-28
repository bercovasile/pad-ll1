using Grpc.Core;
using System.Threading;
using System.Threading.Tasks;

namespace Broker.Application.Abstractions.Consumer
{
    public interface IGrpcConsumerBroker
    {
        Task<IMessageConsumer> AcceptAsync(IAsyncStreamReader<object> clientStream, IServerStreamWriter<object> serverStream, CancellationToken cancellation = default);
    }
}
