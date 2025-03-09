using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class Product : BaseEntity
{
	public string Name { get; set; }
	public string SKU { get; set; } // Stock Keeping Unit - Benzersiz ürün kodu
	public string Description { get; set; }
	public decimal Price { get; set; }
	public string Barcode { get; set; }
	public Guid CompanyId { get; set; }
	public Company Company { get; set; }
	public Guid? ProductFormulaId { get; set; }
	public ProductFormula ProductFormula { get; set; }
	public List<ProductCategory> ProductCategories { get; set; }
}
