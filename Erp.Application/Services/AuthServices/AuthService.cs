using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces;
using Erp.Domain.Interfaces.Jwt;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Services.AuthServices;

public class AuthService : IAuthService
{
	private readonly ErpDbContext _db;
	private readonly IJwtService _jwtService;
	private readonly IPasswordHasher<User> _passwordHasher;

	public AuthService(ErpDbContext db, IJwtService jwtService, IPasswordHasher<User> passwordHasher)
	{
		_db = db;
		_jwtService = jwtService;
		_passwordHasher = passwordHasher;
	}

	public async Task<TokenDto> LoginAsync(UserLoginDto userLoginDto)
	{
		var user = _db.Users.FirstOrDefault(x => x.Email == userLoginDto.Email);
		if (user == null)
		{
			throw new UserAuthException("Email yada şifre yanlış");
		}

		var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
		if (result == PasswordVerificationResult.Failed)
		{
			throw new UserAuthException("Email yada Şifre yanlış");
		}

		var token = _jwtService.GenerateToken(user);
		return token;

	}
}
