﻿using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;
using Erp.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Data;

public class ErpDbContext : DbContext
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	public ErpDbContext(DbContextOptions<ErpDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public DbSet<User> Users { get; set; }
	public DbSet<PendingUser> PendingUsers { get; set; }
	public DbSet<Company> Companies { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<Category> Categories { get; set; }
	public DbSet<ProductCategory> ProductCategories { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<OrderItem> OrderItem { get; set; }
	public DbSet<OrderPayment> OrderPayment { get; set; }
	public DbSet<Discount> Discount { get; set; }
	public DbSet<Unit> Units { get; set; }
	public DbSet<Supplier> Suppliers { get; set; }
	public DbSet<RawMaterial> RawMaterials { get; set; }
	public DbSet<RawMaterialSupplier> RawMaterialSuppliers { get; set; }
	public DbSet<ProductFormula> ProductFormulas { get; set; }
	public DbSet<ProductFormulaItem> ProductFormulaItems { get; set; }

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var userId = _httpContextAccessor.HttpContext?.User.FindFirst(CustomClaims.Id)?.Value;
		Guid? userGuid = null;
		if (!string.IsNullOrEmpty(userId))
		{
			userGuid = Guid.Parse(userId);
		}
		var entries = ChangeTracker.Entries<BaseEntity>()
			.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);
	
		foreach (var entry in entries)
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = DateTime.UtcNow;
				entry.Entity.IsDeleted = false;
				entry.Entity.CreatedById = userGuid;
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.UpdatedAt = DateTime.UtcNow;
				entry.Entity.UpdatedById = userGuid;
			}
		}

		return base.SaveChangesAsync(cancellationToken);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var options = new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		modelBuilder.Entity<User>()
			.Property(e => e.Customer)
		.HasColumnType("jsonb")
			.HasConversion(
		v => JsonSerializer.Serialize(v, options),
				v => JsonSerializer.Deserialize<Customer>(v, options) ?? new Customer());
		base.OnModelCreating(modelBuilder);
	}
}
