using Erp.Domain.Entities;
using Erp.Domain.Interfaces.Jwt;
using Erp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Erp.Infrastructure.Configurations;

public static class AuthenticationExtensions
{
	public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
	{
		// Password Hasher
		services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

		// JWT Service
		services.AddScoped<IJwtService, JwtService>();

		// JWT Authentication
		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = configuration["Jwt:Issuer"],
					ValidAudience = configuration["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]))
				};
			});

		return services;
	}
}