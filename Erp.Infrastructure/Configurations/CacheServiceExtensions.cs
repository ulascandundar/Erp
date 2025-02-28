using Castle.DynamicProxy;
using Erp.Infrastructure.Interceptors.Caching;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

public static class CacheServiceExtensions
{
	public static IServiceCollection AddCaching(this IServiceCollection services)
	{
		// Register memory cache
		services.AddMemoryCache();

		// Register cache interceptor
		services.AddSingleton<CacheInterceptor>();

		// Register proxy generator
		services.AddSingleton<ProxyGenerator>();

		return services;
	}

	public static IServiceCollection AddCachedService<TService, TImplementation>(
		this IServiceCollection services,
		ServiceLifetime lifetime = ServiceLifetime.Scoped)
		where TService : class
		where TImplementation : class, TService
	{
		// Register the original service implementation
		services.Add(new ServiceDescriptor(
			typeof(TImplementation),
			typeof(TImplementation),
			lifetime));

		// Register the proxied service
		services.Add(new ServiceDescriptor(
			typeof(TService),
			serviceProvider =>
			{
				var service = serviceProvider.GetRequiredService<TImplementation>();
				var interceptor = serviceProvider.GetRequiredService<CacheInterceptor>();
				var generator = serviceProvider.GetRequiredService<ProxyGenerator>();

				// Create a proxy for the service
				return generator.CreateInterfaceProxyWithTargetInterface<TService>(
					service,
					interceptor);
			},
			lifetime));

		return services;
	}
}
