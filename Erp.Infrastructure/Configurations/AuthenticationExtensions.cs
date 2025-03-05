using Erp.Domain.Entities;
using Erp.Domain.Interfaces.Jwt;
using Erp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.Tasks;

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
				
				// SignalR için JWT yapılandırması
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = context =>
					{
						// URL'den token'ı al
						var accessToken = context.Request.Query["access_token"];
						
						// Header'dan token'ı al (eğer URL'de yoksa)
						if (string.IsNullOrEmpty(accessToken))
						{
							accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
						}
						
						// Path'i kontrol et
						var path = context.HttpContext.Request.Path;
						if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
						{
							// Token'ı ayarla
							context.Token = accessToken;
						}
						return Task.CompletedTask;
					}
				};
			});

		return services;
	}
}