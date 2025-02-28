using Erp.Application.Services.AccountServices;
using Erp.Application.Services.AuthServices;
using Erp.Application.Services.UserServices;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Configurations;

public static class ServiceRegister
{
	public static IServiceCollection RegisterServices(this IServiceCollection services)
	{ 
		services.AddScoped<IUserService, UserService>(); 
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		return services;
	}
}
