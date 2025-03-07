using Erp.Application.Services.AccountServices;
using Erp.Application.Services.AuthServices;
using Erp.Application.Services.CategoryServices;
using Erp.Application.Services.CompanyServices;
using Erp.Application.Services.LocalizationServices;
using Erp.Application.Services.NotificationServices;
using Erp.Application.Services.OrderServices;
using Erp.Application.Services.ProductServices;
using Erp.Application.Services.UserServices;
using Erp.Application.Validators.CategoryValidator;
using Erp.Application.Validators.OrderValidator;
using Erp.Application.Validators.ProductValidator;
using Erp.Domain.DTOs.Order;
using Erp.Domain.Interfaces.BusinessServices;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Erp.Application.Configurations;

public static class ServiceRegister
{
	public static IServiceCollection RegisterServices(this IServiceCollection services)
	{
		services.AddScoped<ILocalizationService, LocalizationService>();
		services.AddScoped<IUserService, UserService>(); 
		services.AddScoped<IAuthService, AuthService>();
		services.AddScoped<ICurrentUserService, CurrentUserService>();
		services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
		services.AddScoped<IPendingUserService, PendingUserService>();
		services.AddScoped<ICompanyService, CompanyService>();
		services.AddScoped<IProductService, ProductService>();
		services.AddScoped<ICategoryService, CategoryService>();
		services.AddScoped<IPlaceOrderService, PlaceOrderService>();
		services.AddScoped<ISignalRNotificationService, SignalRNotificationService>();
		return services;
	}
}
