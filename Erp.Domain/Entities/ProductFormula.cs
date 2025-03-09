using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class ProductFormula : BaseEntity
{
	public string Name { get; set; }
	public string Description { get; set; }
	public List<Product> Products { get; set; }
	public List<ProductFormulaItem> Items { get; set; }
}
