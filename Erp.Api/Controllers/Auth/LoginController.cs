using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Auth;
public class LoginController : BaseV1Controller
{
	private readonly IAuthService _authService;
	public LoginController(IAuthService authService)
	{
		_authService = authService;
	}

	[HttpPost]
	public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
	{
		var result = await _authService.LoginAsync(userLoginDto);
		return CustomResponse(result);
	}
}
