using Erp.Api.Controllers.Abstracts;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.Account;

[Authorize]
public class CurrentUserController : BaseV1Controller
{
	private readonly ICurrentUserService _currentUserService;

	public CurrentUserController(ICurrentUserService currentUserService)
	{
		_currentUserService = currentUserService;
	}

	[HttpGet]
	public IActionResult GetCurrentUser()
	{
		var result = _currentUserService.GetCurrentUser();
		return CustomResponse(result);
	}
}
