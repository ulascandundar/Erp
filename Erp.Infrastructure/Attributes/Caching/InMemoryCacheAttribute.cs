using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Attributes.Caching;
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class InMemoryCacheAttribute : Attribute
{
	public string CacheKey { get; }
	public int ExpirationMinutes { get; }

	public InMemoryCacheAttribute(string cacheKey = null, int expirationMinutes = 30)
	{
		CacheKey = cacheKey;
		ExpirationMinutes = expirationMinutes;
	}
}
