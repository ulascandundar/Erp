using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class ProductFormulaItem : BaseEntity
{
	public decimal Quantity { get; set; }
	public Guid ProductFormulaId { get; set; }
	public ProductFormula ProductFormula { get; set; }
	public Guid RawMaterialId { get; set; }
	public RawMaterial RawMaterial { get; set; }
	public Guid UnitId { get; set; }
	public Unit Unit { get; set; }
}
