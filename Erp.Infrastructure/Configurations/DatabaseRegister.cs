using Erp.Infrastructure.Data;
using Erp.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

public static class DatabaseRegister
{
	public static IServiceCollection RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		// Database Context
#if DEBUG
		// Use in-memory database in Debug mode
		services.AddDbContext<ErpDbContext>(options =>
			options.UseInMemoryDatabase("ErpInMemoryDb"));
		Console.WriteLine("Using in-memory database for debugging");
#else
		services.AddDbContext<ErpDbContext>(options =>
			options.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection"),
				x => x.MigrationsAssembly("Erp.Infrastructure")));
#endif
		// Seed Service
		services.AddScoped<SeedService>();

		return services;
	}
}
