using Erp.Domain.DTOs.Auth;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.Jwt;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
namespace Erp.Infrastructure.Services;

public class JwtService : IJwtService
{
	private readonly IConfiguration _configuration;
	private readonly string _secretKey;
	private readonly string _issuer;
	private readonly string _audience;
	private readonly int _expirationInMinutes;

	public JwtService(IConfiguration configuration)
	{
		_configuration = configuration;
		_secretKey = _configuration["Jwt:SecretKey"];
		_issuer = _configuration["Jwt:Issuer"];
		_audience = _configuration["Jwt:Audience"];
		_expirationInMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"]);
	}

	public TokenDto GenerateToken(User user)
	{
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
		var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

		var claims = new List<Claim>
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Email, user.Email),
			};

		// Add roles
		foreach (var role in user.Roles)
		{
			claims.Add(new Claim(ClaimTypes.Role, role));
		}

		var token = new JwtSecurityToken(
			issuer: _issuer,
			audience: _audience,
			claims: claims,
			expires: DateTime.UtcNow.AddMinutes(_expirationInMinutes),
			signingCredentials: credentials
		);

		var stringToken = new JwtSecurityTokenHandler().WriteToken(token);

		return new TokenDto
		{
			Token = stringToken,
			Expiration = token.ValidTo,
			Email = user.Email,
			Roles = user.Roles
		};
	}

	public bool ValidateToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var key = Encoding.UTF8.GetBytes(_secretKey);

		try
		{
			tokenHandler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = _issuer,
				ValidateAudience = true,
				ValidAudience = _audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero
			}, out SecurityToken validatedToken);

			return true;
		}
		catch
		{
			return false;
		}
	}

	public IDictionary<string, string> GetClaimsFromToken(string token)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

		return jwtToken?.Claims.ToDictionary(
			claim => claim.Type,
			claim => claim.Value
		);
	}
}
