﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.RawMaterial;

public class RawMaterialDto
{
	public Guid Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public decimal Stock { get; set; }
	public string Barcode { get; set; }
	public Guid CompanyId { get; set; }
	public Guid UnitId { get; set; }
}
