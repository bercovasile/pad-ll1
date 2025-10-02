using System.Threading.Tasks;
using Grpc.Core;

namespace Broker.Grpc
{
    // Minimal stub for generated gRPC service to allow compilation when generated code is not present.
    public class BrokerReceiver
    {
        public abstract class BrokerReceiverBase
        {
            public virtual Task StreamMessages(
                IAsyncStreamReader<Broker.Context.Messages.MessageRequest> requestStream,
                IServerStreamWriter<Broker.Context.Response.Response> responseStream,
                ServerCallContext context)
            {
                return Task.CompletedTask;
            }
        }
    }
}
