using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.DTOs.Unit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductFormula;

public class ProductFormulaItemDto
{
    public Guid Id { get; set; }
    public Guid RawMaterialId { get; set; }
    public RawMaterialDto RawMaterial { get; set; }
    public decimal Quantity { get; set; }
    public Guid UnitId { get; set; }
    public UnitDto Unit { get; set; }
} 