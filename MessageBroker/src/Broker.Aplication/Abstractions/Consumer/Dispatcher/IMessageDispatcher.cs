using System.Threading;
using System.Threading.Tasks;
using Broker.Domain.Entites.Messages;
using Broker.Domain.Entites.Dispatcher;

namespace Broker.Application.Abstractions.Dispatcher
{
    public interface IMessageDispatcher
    {
        Task<MessageDispatchResult> DispatchAsync(Message message, CancellationToken cancellation);
    }
}
