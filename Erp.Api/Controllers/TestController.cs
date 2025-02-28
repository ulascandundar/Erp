using Erp.Api.Controllers.Abstracts;
using Erp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Erp.Api.Controllers;
public class TestController : BaseV1Controller
{
	private readonly IMemoryCache _memoryCache;

	public TestController(IMemoryCache memoryCache)
	{
		_memoryCache = memoryCache;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		_memoryCache.Remove(CacheKeys.AllUsers);
		return CustomResponse();
	}
}
