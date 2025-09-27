using Broker.Application.Abstractions.Receiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Presentation.Abstractions;

public interface ITcpSocketBroker
{
	Task<IBrokerReceiver?> AcceptSocketAsync(CancellationToken cancellation = default);
}
