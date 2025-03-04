using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class Category : BaseEntity
{
	public Guid CompanyId { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public List<ProductCategory> ProductCategories { get; set; }
}
