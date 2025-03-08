using Erp.Domain.Entities;
using Erp.Domain.Enums;
using Erp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Services;

public class SeedService
{
	private readonly ErpDbContext _context;
	private readonly IPasswordHasher<User> _passwordHasher;
	private readonly IConfiguration _configuration;

	public SeedService(
		ErpDbContext context,
		IPasswordHasher<User> passwordHasher,
		IConfiguration configuration)
	{
		_context = context;
		_passwordHasher = passwordHasher;
		_configuration = configuration;
	}

	public async Task SeedDataAsync()
	{
		try
		{
			await GenerateUserAsync();


		}
		catch (Exception ex)
		{
			// Log the error but don't throw - we don't want to prevent app startup
			Console.WriteLine($"Error seeding database: {ex.Message}");
		}
	}

	private async Task GenerateUserAsync()
	{
		if (!await _context.Users.AnyAsync(u => u.Email == "admin@starter.com"))
		{
			var adminUser = new User
			{
				Id = Guid.NewGuid(),
				Email = "admin@starter.com",
				CreatedAt = DateTime.UtcNow,
				Roles = new List<string> { Roles.User, Roles.Admin }
			};

			// Get admin password from configuration or use default
			var adminPassword = "Admin123!";
			adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, adminPassword);

			await _context.Users.AddAsync(adminUser);
			await _context.SaveChangesAsync();
		}
	}

	private async Task GenerateGlobalUnits()
	{
		if (!await _context.Units.AnyAsync(o=>o.Name == "Gram" && o.IsGlobal && !o.IsDeleted))
		{

		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Mililitre" && o.IsGlobal && !o.IsDeleted))
		{

		}
	}
}