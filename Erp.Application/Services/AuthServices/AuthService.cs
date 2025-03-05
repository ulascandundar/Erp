using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Interfaces.Jwt;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Erp.Domain.Constants;

namespace Erp.Application.Services.AuthServices;

public class AuthService : IAuthService
{
	private readonly ErpDbContext _db;
	private readonly IJwtService _jwtService;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly ILocalizationService _localizationService;

	public AuthService(
		ErpDbContext db, 
		IJwtService jwtService, 
		IPasswordHasher<User> passwordHasher,
		ILocalizationService localizationService)
	{
		_db = db;
		_jwtService = jwtService;
		_passwordHasher = passwordHasher;
		_localizationService = localizationService;
	}

	public async Task<TokenDto> LoginAsync(UserLoginDto userLoginDto)
	{
		var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == userLoginDto.Email);
		if (user == null)
		{
			throw new UserAuthException(_localizationService.GetLocalizedString(ResourceKeys.Errors.InvalidCredentials));
		}

		var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, userLoginDto.Password);
		if (result == PasswordVerificationResult.Failed)
		{
			throw new UserAuthException(_localizationService.GetLocalizedString(ResourceKeys.Errors.InvalidCredentials));
		}

		var token = _jwtService.GenerateToken(user);
		return token;
	}
}
