using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.CustomerControllers.Auth;
public class ForgotPasswordController : BaseV1Controller
{
	private readonly IForgotPasswordService _forgotPasswordService;
	public ForgotPasswordController(IForgotPasswordService forgotPasswordService)
	{
		_forgotPasswordService = forgotPasswordService;
	}

	[HttpPost("send")]
	public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
	{
		var result = await _forgotPasswordService.ForgotPasswordAsync(forgotPasswordDto);
#if DEBUG
		return CustomResponse(result);
#endif
		return CustomResponse(result);
	}

	[HttpPut("reset-password")]
	public async Task<IActionResult> ResetPassword([FromBody] ForgotPasswordWithPasswordDto forgotPasswordWithPasswordDto)
	{
		await _forgotPasswordService.VerifyOtpAsync(forgotPasswordWithPasswordDto);
		return CustomResponse();
	}
}
