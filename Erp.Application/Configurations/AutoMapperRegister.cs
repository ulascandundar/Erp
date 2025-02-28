using Erp.Application.Mappings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Configurations;

public static class AutoMapperRegister
{
	public static IServiceCollection RegisterAutoMapper(this IServiceCollection services)
	{
		services.AddAutoMapper(typeof(UserProfile).Assembly);
		return services;
	}
}
