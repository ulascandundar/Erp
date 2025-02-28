using Erp.Application.Validators.AuthValidator;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Configurations;

public static class FluentValidationRegister
{
	public static IServiceCollection RegisterFluentValidation(this IServiceCollection services)
	{
		services.AddFluentValidationAutoValidation(config =>
		{
			// This prevents the automatic validation response
			config.DisableDataAnnotationsValidation = true;
		})
			.AddFluentValidationClientsideAdapters();
		services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();
		return services;
	}
}
