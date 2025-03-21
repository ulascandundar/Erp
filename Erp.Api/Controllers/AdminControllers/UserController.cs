﻿using Erp.Api.Controllers.Abstracts;
using Erp.Application.Services.AccountServices;
using Erp.Domain.DTOs.Customer;
using Erp.Domain.DTOs.Pagination;
using Erp.Domain.DTOs.User;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers.AdminControllers;
[Authorize(Roles = $"{Roles.Admin},{Roles.CompanyAdmin}")]
public class UserController : BaseV1Controller
{
	private readonly IUserService _userService;
	private readonly ICurrentUserService _currentUserService;
	public UserController(IUserService userService, ICurrentUserService currentUserService)
	{
		_userService = userService;
		_currentUserService = currentUserService;
	}

	[Authorize(Roles = Roles.Admin)]
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
	[Authorize(Roles = Roles.Admin)]
	[HttpPut("changePassword/{id}")]
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

	[HttpGet("getCurrentUser")]
	public IActionResult GetCurrentUser()
	{
		var result = _currentUserService.GetCurrentUser();
		return CustomResponse(result);
	}

	[HttpPut("setCustomer")]
	public async Task<IActionResult> SetCustomer([FromBody] CustomerDto customerDto)
	{
		await _userService.SetCustomer(customerDto);
		return CustomResponse();
	}

	[HttpGet("getCustomer")]
	public async Task<IActionResult> GetCustomer()
	{
		var result = await _userService.GetCustomer();
		return CustomResponse(result);
	}

}
