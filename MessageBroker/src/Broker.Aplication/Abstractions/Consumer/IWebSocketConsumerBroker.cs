using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Broker.Application.Abstractions.Consumer
{
    public interface IWebSocketConsumerBroker
    {
        Task<IMessageConsumer?> AcceptAsync(HttpContext context, CancellationToken cancellation = default);
    }
}
