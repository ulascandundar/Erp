using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductFormula;

public class ProductFormulaCreateDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public List<ProductFormulaItemCreateDto> Items { get; set; } = new List<ProductFormulaItemCreateDto>();
}
