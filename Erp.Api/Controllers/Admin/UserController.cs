using Erp.Api.Controllers.Abstracts;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Admin;
[Authorize(Roles = Roles.Admin)]
public class UserController : BaseV1Controller
{
	private readonly IUserService _userService;
	public UserController(IUserService userService)
	{
		_userService = userService;
	}

	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var result = await _userService.GetAllUsersAsync();
		return CustomResponse(result);
	}
}
