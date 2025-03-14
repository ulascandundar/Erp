using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.RawMaterialSupplier;

public class RawMaterialSupplierCreateDto
{
    public Guid RawMaterialId { get; set; }
    public Guid SupplierId { get; set; }
    public decimal Price { get; set; }
} 