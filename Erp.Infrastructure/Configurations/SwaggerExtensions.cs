﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Erp.Infrastructure.Configurations;

public static class SwaggerExtensions
{
	public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
	{
		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Starter API",
				Version = "v1",
				Description = "A starter template for building REST APIs with ASP.NET Core"
			});

			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = "Bearer"
			});

			c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				});
		});

		return services;
	}

	public static IApplicationBuilder UseSwaggerServices(this IApplicationBuilder app)
	{
		app.UseSwagger();
		app.UseSwaggerUI();

		return app;
	}
}
