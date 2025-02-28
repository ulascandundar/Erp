using Microsoft.AspNetCore.Builder;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

public static class LoggingExtensions
{
	
	// Add user ID to logs when authenticated
	public static IApplicationBuilder UseLogUserContext(this IApplicationBuilder app)
	{
		return app.Use(async (context, next) =>
		{
			// Add user ID to log context if user is authenticated
			if (context.User?.Identity?.IsAuthenticated == true)
			{
				LogContext.PushProperty("UserId", context.User.FindFirst("sub")?.Value ?? context.User.Identity.Name);
			}

			await next();
		});
	}
}
