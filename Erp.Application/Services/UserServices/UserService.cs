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

namespace Erp.Application.Services.UserServices;

public class UserService : IUserService
{
	private readonly ErpDbContext _db;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IMapper _mapper;
	public UserService(ErpDbContext db, IPasswordHasher<User> passwordHasher, IMapper mapper)
	{
		_db = db;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
	}

	public async Task<UserDto> CreateUserAsync(UserCreateDto userCreateDto)
	{
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
		var dto = _mapper.Map<UserDto>(user);
		return dto;
	}

	public async Task HardDeleteUserAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		_db.Users.Remove(user);
		await _db.SaveChangesAsync();
	}

	public async Task SoftDeleteUserAsync(Guid id)
	{
		var user = _db.Users.FirstOrDefault(x => x.Id == id);
		user.IsDeleted = true;
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
	}
}
