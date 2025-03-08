using Erp.Domain.DTOs.User;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Services.AccountServices;

public class CurrentUserService : ICurrentUserService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public UserDto GetCurrentUser()
	{
		UserDto user = new UserDto();
		user.Id = Guid.Parse(_httpContextAccessor.HttpContext.User.FindFirst(CustomClaims.Id).Value);
		user.Email = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Email).Value;
		user.Roles = _httpContextAccessor.HttpContext.User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
		
		// Extract CompanyId claim if available
		var companyIdClaim = _httpContextAccessor.HttpContext.User.FindFirst(CustomClaims.CompanyId);
		if (companyIdClaim != null)
		{
			user.CompanyId = Guid.Parse(companyIdClaim.Value);
		}
		
		return user;
	}
}
