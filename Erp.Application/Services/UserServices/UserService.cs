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

namespace Erp.Application.Services.UserServices;

public class UserService : IUserService
{
	private readonly ErpDbContext _db;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IMapper _mapper;
	private readonly ICurrentUserService _currentUserService;
	public UserService(ErpDbContext db, IPasswordHasher<User> passwordHasher, IMapper mapper, ICurrentUserService currentUserService)
	{
		_db = db;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
		_currentUserService = currentUserService;
	}

	public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
	{
		var userExists = _db.Users.Any(x => x.Email == userCreateDto.Email);
		if (userExists)
		{
			throw new UserEmailAlreadyExistsException();
		}
		var user = _mapper.Map<User>(userCreateDto);
		user.PasswordHash = _passwordHasher.HashPassword(user, userCreateDto.Password);
		await _db.Users.AddAsync(user);
		await _db.SaveChangesAsync();
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}
	[InMemoryCache(expirationMinutes: 60,cacheKey:CacheKeys.AllUsers)]
	public Task<List<UserDto>> GetAllUsersAsync()
	{
		var entities = _db.Users.Where(x => x.IsDeleted == false).ToList();
		var dtos = _mapper.Map<List<UserDto>>(entities);
		return Task.FromResult(dtos);
	}

	public async Task<UserDto> GetUserByIdAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException("Kullanıcı bulunamadı");
		}
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}

	public async Task HardDeleteUserAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException("Kullanıcı bulunamadı");
		}
		_db.Users.Remove(user);
		await _db.SaveChangesAsync();
	}

	public async Task SoftDeleteUserAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		if (user == null)
		{
			throw new NullValueException("Kullanıcı bulunamadı");
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
}
