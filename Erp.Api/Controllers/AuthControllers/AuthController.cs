using Erp.Api.Controllers.Abstracts;
using Erp.Application.Services.UserServices;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.DTOs.User;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.AuthControllers;
public class AuthController : BaseV1Controller
{
	private readonly IAuthService _authService;
	private readonly IForgotPasswordService _forgotPasswordService;
	private readonly IUserService _userService;
	public AuthController(IAuthService authService, IForgotPasswordService forgotPasswordService, IUserService userService)
	{
		_authService = authService;
		_forgotPasswordService = forgotPasswordService;
		_userService = userService;
	}

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
	{
		var result = await _authService.LoginAsync(userLoginDto);
		return CustomResponse(result);
	}

	[HttpPost("forgotPassword/send")]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
	{
		var result = await _forgotPasswordService.ForgotPasswordAsync(forgotPasswordDto);
#if DEBUG
		return CustomResponse(result);
#endif
		return CustomResponse(result);
	}

	[HttpPut("ForgotPassword/resetPassword")]
	public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordWithPasswordDto forgotPasswordWithPasswordDto)
	{
		await _forgotPasswordService.VerifyOtpAsync(forgotPasswordWithPasswordDto);
		return CustomResponse();
	}
	[Authorize]
	[HttpPut("changePassword")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordUserDto changePasswordUserDto)
	{
		var result = await _userService.ChangeCurrentUserPasswordAsync(changePasswordUserDto);
		return CustomResponse(result);
	}
}
