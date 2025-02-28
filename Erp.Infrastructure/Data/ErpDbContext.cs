using Erp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Data;

public class ErpDbContext : DbContext
{
	public ErpDbContext(DbContextOptions<ErpDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users { get; set; }
}
