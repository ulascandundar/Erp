using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.Entities;

public class RawMaterialSupplier : BaseEntity
{
    public Guid RawMaterialId { get; set; }
    public RawMaterial RawMaterial { get; set; }
    public Guid SupplierId { get; set; }
    public Supplier Supplier { get; set; }
    public decimal Price { get; set; }
} 