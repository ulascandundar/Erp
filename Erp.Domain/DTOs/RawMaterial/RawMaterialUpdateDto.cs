using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Erp.Domain.DTOs.RawMaterial;

public class RawMaterialUpdateDto
{
	public string Name { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public string Barcode { get; set; }
	public Guid UnitId { get; set; }
}
