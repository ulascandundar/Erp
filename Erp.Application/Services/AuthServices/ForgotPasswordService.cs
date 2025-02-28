using Erp.Domain.CustomExceptions;
using Erp.Domain.DTOs.Auth;
using Erp.Domain.Entities;
using Erp.Domain.Interfaces.BusinessServices;
using Erp.Domain.Utils;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Application.Services.AuthServices;

public class ForgotPasswordService : IForgotPasswordService
{
	private readonly ErpDbContext _db;
	private readonly IPasswordHasher<User> _passwordHasher;


	public ForgotPasswordService(ErpDbContext db, IPasswordHasher<User> passwordHasher)
	{
		_db = db;
		_passwordHasher = passwordHasher;
	}


	public async Task<string> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
	{
		var now = DateTime.UtcNow;
		var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
		if (user == null)
		{
			throw new NullValueException("Kullanıcı bulunamadı.");
		}
		// Generate a new password
		var otp = StringUtil.GenerateRandomOtpCode();
		user.ForgotPasswordOtp = otp;
		user.ForgotPasswordOtpExpireDate = now.AddMinutes(2);
		// Save the new password
		await _db.SaveChangesAsync();
		// Send the new password to the user
		// SendEmail(user.Email, newPassword);
		return otp;
	}

	public async Task VerifyOtpAsync(ForgotPasswordWithPasswordDto forgotPasswordWithPasswordDto)
	{
		var now = DateTime.UtcNow;
		var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordWithPasswordDto.Email);
		if (user == null)
		{
			throw new NullValueException("Geçersiz kod");
		}
		if (user.ForgotPasswordOtp != forgotPasswordWithPasswordDto.Otp || user.ForgotPasswordOtpExpireDate < now)
		{
			throw new NullValueException("Geçersiz kod");
		}
		user.PasswordHash = _passwordHasher.HashPassword(user, forgotPasswordWithPasswordDto.Password);
		user.ForgotPasswordOtp = null;
		user.ForgotPasswordOtpExpireDate = null;
		_db.Users.Update(user);
		await _db.SaveChangesAsync();
	}
}
