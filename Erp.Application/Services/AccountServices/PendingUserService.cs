using AutoMapper;
using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.DTOs.PenginUser;
using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;
using Erp.Domain.Enums;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Interfaces.Jwt;
using Erp.Domain.Models;
using Erp.Domain.Utils;
using Erp.Infrastructure.Data;
using Erp.Notifications.Sender;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Erp.Application.Services.AccountServices;

public class PendingUserService : IPendingUserService
{
	private readonly ErpDbContext _db;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IMapper _mapper;
	private readonly IJwtService _jwtService;
	private readonly NotificationSender _notificationSender;

	public PendingUserService(ErpDbContext db, IPasswordHasher<User> passwordHasher, IMapper mapper,
		IJwtService jwtService, NotificationSender notificationSender)
	{
		_db = db;
		_passwordHasher = passwordHasher;
		_mapper = mapper;
		_jwtService = jwtService;
		_notificationSender = notificationSender;
	}

	public async Task<PendingUserResponseDto> CreateUserAsync(PendingUserCreateDto pendingUserCreateDto)
	{
		var emailExists = await _db.Users.AnyAsync(x => x.Email == pendingUserCreateDto.Email && !x.IsDeleted);
		if (emailExists)
		{
			throw new UserEmailAlreadyExistsException();
		}

		var phoneExists = await _db.Users.AnyAsync(x => x.Customer.PhoneNumber == pendingUserCreateDto.PhoneNumber && !x.IsDeleted);
		if (phoneExists)
		{
			throw new UserPhoneNumberAlreadyExistsException();
		}
		var now = DateTime.UtcNow;
		var pendingUser = _mapper.Map<PendingUser>(pendingUserCreateDto);
		pendingUser.Otp = StringUtil.GenerateRandomOtpCode();
		pendingUser.ExpiryDate = now.AddMinutes(2);
		await _db.PendingUsers.AddAsync(pendingUser);
		await _db.SaveChangesAsync();
		await _notificationSender.StartAsync(new NotificationMessage
		{
			Type = NotificationTypes.Email,
			Subject = "Doğrulama Kodu",
			Content = $"Doğrulama kodunuz: {pendingUser.Otp}",
			Recipient = pendingUser.Email
		});
		var dto = _mapper.Map<PendingUserResponseDto>(pendingUser);
		return dto;
	}

	public async Task<TokenDto> VerifyOtpAsync(PendingUserVerifyOtpDto pendingUserVerifyOtpDto)
	{
		var pendingUser = await _db.PendingUsers.FirstOrDefaultAsync(x => x.Otp == pendingUserVerifyOtpDto.Otp &&
		x.Id == pendingUserVerifyOtpDto.PendingUserId && x.ExpiryDate > DateTime.UtcNow);
		if (pendingUser == null)
		{
			throw new NullValueException("Doğrulama kodu geçersiz");
		}
		var emailExists = await _db.Users.AnyAsync(x => x.Email == pendingUser.Email && !x.IsDeleted);
		if (emailExists)
		{
			throw new UserEmailAlreadyExistsException();
		}

		var phoneExists = await _db.Users.AnyAsync(x => x.Customer.PhoneNumber == pendingUser.PhoneNumber && !x.IsDeleted);
		if (phoneExists)
		{
			throw new UserPhoneNumberAlreadyExistsException();
		}
		User user = new User
		{
			Email = pendingUser.Email,
			Customer = new Customer
			{
				PhoneNumber = pendingUser.PhoneNumber,
				Address = pendingUser.Address,
				City = pendingUser.City,
				FirstName = pendingUser.FirstName,
				LastName = pendingUser.LastName,
				Tckn = pendingUser.Tckn,
			},
			Roles = new List<string> { "User" }
		};
		user.PasswordHash = _passwordHasher.HashPassword(user, pendingUser.Password);
		await _db.Users.AddAsync(user);
		await _db.SaveChangesAsync();
		var token = _jwtService.GenerateToken(user);
		return token;
	}
}
