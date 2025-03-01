using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.PenginUser;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Account;

public class PendingUserController : BaseV1Controller
{
	private readonly IPendingUserService _pendingUserService;

	public PendingUserController(IPendingUserService pendingUserService)
	{
		_pendingUserService = pendingUserService;
	}
	[HttpPost("create")]
	public async Task<IActionResult> CreateUserAsync([FromBody] PendingUserCreateDto pendingUserCreateDto)
	{
		var result = await _pendingUserService.CreateUserAsync(pendingUserCreateDto);
		return Ok(result);
	}

	[HttpPost("verify")]
	public async Task<IActionResult> VerifyOtpAsync([FromBody] PendingUserVerifyOtpDto pendingUserVerifyOtpDto)
	{
		var result = await _pendingUserService.VerifyOtpAsync(pendingUserVerifyOtpDto);
		return Ok(result);
	}
}
