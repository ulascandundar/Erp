using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductFormula;

public class ProductFormulaUpdateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<ProductFormulaItemUpdateDto> Items { get; set; } = new List<ProductFormulaItemUpdateDto>();
} 