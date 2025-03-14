using Erp.Domain.DTOs.RawMaterial;
using Erp.Domain.DTOs.Supplier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.RawMaterialSupplier;

public class RawMaterialSupplierDto
{
    public Guid Id { get; set; }
    public Guid RawMaterialId { get; set; }
    public RawMaterialDto RawMaterial { get; set; }
    public Guid SupplierId { get; set; }
    public SupplierDto Supplier { get; set; }
    public decimal Price { get; set; }
} 