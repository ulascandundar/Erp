﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.ProductFormula;

public class ProductFormulaItemCreateDto
{
    public Guid RawMaterialId { get; set; }
    public decimal Quantity { get; set; }
    public Guid UnitId { get; set; }
}
