using Erp.Infrastructure.Middlewares;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

public static class GlobalExceptionHandlingMiddlewareExtensions
{
	public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<GlobalExceptionHandlingMiddleware>();
	}
}
