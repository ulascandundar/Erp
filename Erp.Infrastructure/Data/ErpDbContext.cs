using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Data;

public class ErpDbContext : DbContext
{
	public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users { get; set; }

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		var entries = ChangeTracker.Entries<BaseEntity>()
			.Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

		foreach (var entry in entries)
		{
			if (entry.State == EntityState.Added)
			{
				entry.Entity.CreatedAt = DateTime.UtcNow;
				entry.Entity.IsDeleted = false;
			}
			else if (entry.State == EntityState.Modified)
			{
				entry.Entity.UpdatedAt = DateTime.UtcNow;
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
