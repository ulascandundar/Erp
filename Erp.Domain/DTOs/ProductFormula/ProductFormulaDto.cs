using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductFormula;

public class ProductFormulaDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid CompanyId { get; set; }
    public List<ProductFormulaItemDto> Items { get; set; } = new List<ProductFormulaItemDto>();
} 