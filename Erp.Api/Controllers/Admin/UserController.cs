using Erp.Api.Controllers.Abstracts;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.User;
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

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(Guid id)
	{
		var result = await _userService.GetUserByIdAsync(id);
		return CustomResponse(result);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(Guid id)
	{
		await _userService.SoftDeleteUserAsync(id);
		return CustomResponse();
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody]UserCreateDto userCreateDto)
	{
		var result = await _userService.CreateUserAsync(userCreateDto);
		return CustomResponse(result);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update([FromBody]UserUpdateDto userUpdateDto, Guid id)
	{
		var result = await _userService.UpdateUserAsync(userUpdateDto, id);
		return CustomResponse(result);
	}

	[HttpPut("change-password/{id}")]
	public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordUserDto changePasswordUserDto, Guid id)
	{
		var result = await _userService.ChangePasswordUserAsync(changePasswordUserDto, id);
		return CustomResponse(result);
	}

	[HttpGet("paged")]
	public async Task<IActionResult> GetPaged([FromQuery] PaginationRequest paginationRequest)
	{
		var result = await _userService.GedPagedAsync(paginationRequest);
		return CustomResponse(result);
	}

}
