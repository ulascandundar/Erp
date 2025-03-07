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
		services.AddDbContext<ErpDbContext>(options =>
			options.UseNpgsql(
				configuration.GetConnectionString("DefaultConnection"),
				x => x.MigrationsAssembly("Erp.Infrastructure")));
		// Seed Service
		services.AddScoped<SeedService>();

		return services;
	}
}
