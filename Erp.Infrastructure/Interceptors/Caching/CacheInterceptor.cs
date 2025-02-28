using Castle.DynamicProxy;
using Erp.Infrastructure.Attributes.Caching;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Interceptors.Caching;

public class CacheInterceptor : IInterceptor
{
	private readonly IMemoryCache _memoryCache;

	public CacheInterceptor(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	public void Intercept(IInvocation invocation)
	{
		// İmplementasyon metodunu bul
		var implType = invocation.TargetType;
		var implMethod = implType.GetMethod(
			invocation.Method.Name,
			invocation.Method.GetParameters().Select(p => p.ParameterType).ToArray());

		// İmplementasyon metodundaki attribute'ü kontrol et
		var cacheAttribute = implMethod?.GetCustomAttribute<InMemoryCacheAttribute>();

		if (cacheAttribute == null)
		{
			invocation.Proceed();
			return;
		}

		// Generate cache key if not specified
		string cacheKey = cacheAttribute.CacheKey;

		if (string.IsNullOrEmpty(cacheKey))
		{
			cacheKey = $"{implType.FullName}.{implMethod.Name}";
		}
		else
		{
			// Format the key with method arguments if needed
			if (invocation.Arguments.Length > 0 && cacheKey.Contains("{"))
			{
				cacheKey = string.Format(cacheKey, invocation.Arguments);
			}
		}

		// Check if method is async
		bool isAsync = invocation.Method.ReturnType.IsGenericType &&
					  invocation.Method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>);

		// Try to get from cache
		if (_memoryCache.TryGetValue(cacheKey, out var cachedResult))
		{
			if (isAsync)
			{
				// For async methods, wrap the cached result in a Task
				var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
				var tcs = typeof(Task).GetMethod(nameof(Task.FromResult))
					.MakeGenericMethod(resultType)
					.Invoke(null, new[] { cachedResult });
				invocation.ReturnValue = tcs;
			}
			else
			{
				invocation.ReturnValue = cachedResult;
			}
			return;
		}

		// Execute the original method
		invocation.Proceed();

		// Handle caching of the result
		if (isAsync)
		{
			var task = (Task)invocation.ReturnValue;
			var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
			
			var newTask = typeof(CacheInterceptor)
				.GetMethod(nameof(CreateCachedTaskResultAsync), BindingFlags.NonPublic | BindingFlags.Instance)
				.MakeGenericMethod(resultType)
				.Invoke(this, new object[] { task, cacheKey, cacheAttribute.ExpirationMinutes });
			
			invocation.ReturnValue = newTask;
		}
		else
		{
			// Store the result in cache for non-async methods
			_memoryCache.Set(
				cacheKey,
				invocation.ReturnValue,
				new MemoryCacheEntryOptions
				{
					AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheAttribute.ExpirationMinutes)
				});
		}
	}

	private async Task<T> CreateCachedTaskResultAsync<T>(Task task, string cacheKey, int expirationMinutes)
	{
		await task.ConfigureAwait(false);
		
		// Get the result using reflection
		var result = (T)((dynamic)task).Result;

		// Cache the result
		_memoryCache.Set(
			cacheKey,
			result,
			new MemoryCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
			});

		return result;
	}
}