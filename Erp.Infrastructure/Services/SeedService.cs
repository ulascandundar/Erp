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
			await GenerateGlobalUnits();

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
			var unit = new Unit
			{
				Name = "Gram",
				ShortCode = "gr",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Weight
			};
			await _context.Units.AddAsync(unit);
		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Mililitre" && o.IsGlobal && !o.IsDeleted))
		{
			var unit = new Unit
			{
				Name = "Mililitre",
				ShortCode = "ml",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Volume
			};
			await _context.Units.AddAsync(unit);
		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Santimetre" && o.IsGlobal && !o.IsDeleted))
		{
			var unit = new Unit
			{
				Name = "Santimetre",
				ShortCode = "cm",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Length
			};
			await _context.Units.AddAsync(unit);
		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Adet" && o.IsGlobal && !o.IsDeleted))
		{
			var unit = new Unit
			{
				Name = "Adet",
				ShortCode = "ad",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Count
			};
			await _context.Units.AddAsync(unit);
		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Saniye" && o.IsGlobal && !o.IsDeleted))
		{
			var unit = new Unit
			{
				Name = "Saniye",
				ShortCode = "sn",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Time
			};
			await _context.Units.AddAsync(unit);
		}
		if (!await _context.Units.AnyAsync(o => o.Name == "Cm2"))
		{
			var unit = new Unit
			{
				Name = "Cm2",
				ShortCode = "cm2",
				ConversionRate = 1,
				IsGlobal = true,
				UnitType = UnitType.Area
			};
			await _context.Units.AddAsync(unit);
		}
		await _context.SaveChangesAsync();
	}
}