
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Broker.Application;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

		return services;
	}

}

