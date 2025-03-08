using Erp.Domain.DTOs.User;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Infrastructure.Data;
using Erp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.CustomExceptions;
using Erp.Infrastructure.Attributes.Caching;
using Erp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using Erp.Domain.DTOs.Pagination;
using Erp.Application.Common.Extensions;
using Erp.Domain.Constants;
using Erp.Domain.Entities.NoSqlEntities;
using Erp.Domain.DTOs.Customer;

namespace Erp.Application.Services.UserServices;

public class UserService : IUserService
{
	private readonly ErpDbContext _db;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IMapper _mapper;
	private readonly ICurrentUserService _currentUserService;
	private readonly ILocalizationService _localizationService;
	
	public UserService(
		ErpDbContext db, 
		IPasswordHasher<User> passwordHasher, 
		IMapper mapper, 
		ICurrentUserService currentUserService,
		ILocalizationService localizationService)
	{
		_db = db;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
		_currentUserService = currentUserService;
		_localizationService = localizationService;
	}

	public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var userExists = _db.Users.Any(x => x.Email == userCreateDto.Email);
		if (userExists)
		{
			throw new UserEmailAlreadyExistsException();
		}
		if (!currentUser.Roles.Contains(Roles.Admin))
		{
			userCreateDto.Roles = new List<string> { Roles.CompanyAdmin };
			userCreateDto.CompanyId = currentUser.CompanyId;
		}
		var user = _mapper.Map<User>(userCreateDto);
		user.PasswordHash = _passwordHasher.HashPassword(user, userCreateDto.Password);
		await _db.Users.AddAsync(user);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}
	
	[InMemoryCache(expirationMinutes: 60,cacheKey:CacheKeys.AllUsers)]
	public async Task<List<UserDto>> GetAllUsersAsync()
	{
		var entities = await _db.Users.Where(x => x.IsDeleted == false).ToListAsync();
		var dtos = _mapper.Map<List<UserDto>>(entities);
		return dtos;
	}

	public async Task<UserDto> GetUserByIdAsync(Guid id)
	{
		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotFound));
		}
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}

	public async Task HardDeleteUserAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotFound));
		}
		_db.Users.Remove(user);
		await _db.SaveChangesAsync();
	}

	public async Task SoftDeleteUserAsync(Guid id)
	{
		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException(_localizationService.GetLocalizedString(ResourceKeys.Errors.UserNotFound));
		}
		user.IsDeleted = true;
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
	}

	public async Task<UserDto> UpdateUserAsync(UserUpdateDto userUpdateDto, Guid userId)
	{
		var userEntity = await _db.Users.FirstOrDefaultAsync(x => x.Email == userUpdateDto.Email && x.Id != userId);
		if (userEntity == null)
		{
			throw new UserEmailAlreadyExistsException();
		}
		var user = _mapper.Map(userUpdateDto, userEntity);
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}

	public async Task<UserDto> ChangePasswordUserAsync(ChangePasswordUserDto changePasswordUserDto, Guid userId)
	{
		var userEntity = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
		if (userEntity == null)
		{
			throw new UserEmailAlreadyExistsException();
		}
		userEntity.PasswordHash = _passwordHasher.HashPassword(userEntity, changePasswordUserDto.Password);
		_db.Users.Update(userEntity);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UserDto>(userEntity);
		return dto;
	}

	public async Task<UserDto> ChangeCurrentUserPasswordAsync(ChangePasswordUserDto changePasswordUserDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var userEntity = await _db.Users.FirstOrDefaultAsync(x => x.Id == currentUser.Id);
		if (userEntity == null)
		{
			throw new UserEmailAlreadyExistsException();
		}
		userEntity.PasswordHash = _passwordHasher.HashPassword(userEntity, changePasswordUserDto.Password);
		_db.Users.Update(userEntity);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UserDto>(userEntity);
		return dto;
	}

	public async Task<CustomPagedResult<UserDto>> GedPagedAsync(PaginationRequest paginationRequest)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var query = _db.Users.Where(x => x.IsDeleted == false).AsQueryable();
		if (!string.IsNullOrEmpty(paginationRequest.Search))
		{
			query = query.Where(x => x.Email.Contains(paginationRequest.Search));
		}
		if (!currentUser.Roles.Contains(Roles.Admin))
		{
			query = query.Where(x => x.CompanyId == currentUser.CompanyId);
		}
		if (!string.IsNullOrEmpty(paginationRequest.Query))
		{
			query = query.Where(paginationRequest.Query);
		}
		var entityResult = await query.ToPagedResultAsync(paginationRequest);
		var dtos = _mapper.Map<List<UserDto>>(entityResult.Items);
		var result = new CustomPagedResult<UserDto>
		{
			Items = dtos,
			TotalCount = entityResult.TotalCount,
			TotalPages = entityResult.TotalPages,
			PageNumber = entityResult.PageNumber,
			PageSize = entityResult.PageSize
		};
		return result;
	}

	public async Task SetCustomer(CustomerDto customerDto)
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == currentUser.Id);
		var customer = _mapper.Map<Customer>(customerDto);
		user.Customer = customer;
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
	}

	public async Task<CustomerDto> GetCustomer()
	{
		var currentUser = _currentUserService.GetCurrentUser();
		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == currentUser.Id);
		var customer = _mapper.Map<CustomerDto>(user.Customer);
		return customer;
	}
}
