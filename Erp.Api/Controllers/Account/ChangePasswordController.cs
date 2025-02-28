using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.User;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Account;
[Authorize]
public class ChangePasswordController : BaseV1Controller
{
	private readonly IUserService _userService;
	public ChangePasswordController(IUserService userService)
	{
		_userService = userService;
	}
	[HttpPut]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordUserDto changePasswordUserDto)
	{
		var result = await _userService.ChangeCurrentUserPasswordAsync(changePasswordUserDto);
		return CustomResponse(result);
	}
}
