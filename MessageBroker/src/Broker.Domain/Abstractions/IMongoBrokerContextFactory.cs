using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Broker.Domain.Abstractions;

public interface IMongoBrokerContextFactory
{
	IMongoBrokerContext GetContext();
	void ReturnContext(IMongoBrokerContext context);
	Task<T> ExecuteAsync<T>(Func<IMongoBrokerContext, Task<T>> operation);
	Task ExecuteAsync(Func<IMongoBrokerContext, Task> operation);
}
